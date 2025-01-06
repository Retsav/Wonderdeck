using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using Zenject;
using Random = System.Random;

public class BlackjackLogic : NetworkBehaviour
{
   private DeckConfig _deckConfig;
   private BlackjackConfig _blackjackConfig;
   
   private List<CardClientData> _clientCardDataFirstPlayer = new List<CardClientData>();
   private List<CardClientData> _clientCardDataSecondPlayer = new List<CardClientData>();
   
   private Random _random = new Random();

   private IBlackjackService _blackjackService;
   private IInventoryService _inventoryService;

   private bool _firstPlayerFinishedTurn;
   private bool _secondPlayerFinishedTurn;
   private bool _eventsInitialized;
   

   [Inject]
   private void ResolveDependencies(IBlackjackService blackjackService, IInventoryService inventoryService)
   {
      _blackjackService = blackjackService;
      _inventoryService = inventoryService;
   }
   
   public override void OnStartClient()
   {
      if (NetworkManager.ClientManager.Connection.IsHost)
      {
         _deckConfig = DebugConfigLoader.Instance.GetConfig<DeckConfig>();
         _blackjackConfig = DebugConfigLoader.Instance.GetConfig<BlackjackConfig>();
         _blackjackService.OrginalDeck = new List<string>();
         _blackjackService.CurrentDeck = new List<string>();
         for (int i = 0; i < _deckConfig.cards.Count; i++) _blackjackService.OrginalDeck.Add(_deckConfig.cards[i].CardId);
         _blackjackService.CurrentDeck = _blackjackService.OrginalDeck;
      }
      StartGameServerRpc();
   }

   private void OnPlayerConnectionStateChanged(NetworkConnection conn, RemoteConnectionStateArgs args)
   {
      if (args.ConnectionState == RemoteConnectionState.Stopped)
      {
         Debug.Log($"Player {conn.ClientId} disconnected.");
      } else if (args.ConnectionState == RemoteConnectionState.Started)
      {
         Debug.Log($"Player {conn.ClientId} connected. Attempting to start the game");
         if (AreBothPlayersConnected()) StartGameServerRpc();
      }
   }


   [ServerRpc(RequireOwnership = false)]
   private void StartGameServerRpc()
   {
      if (!AreBothPlayersConnected()) return;
      _blackjackService.FirstPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
      _blackjackService.SecondPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
      StartGameObserverRpc(BlackjackState.Player1Turn);
      _clientCardDataFirstPlayer = CreateCardData(_blackjackService.FirstPlayerCards);
      _clientCardDataSecondPlayer = CreateCardData(_blackjackService.SecondPlayerCards);
      DealCardsObserverRpc(_clientCardDataFirstPlayer.ToList(), _clientCardDataSecondPlayer.ToList());
      _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(PlayerType.Player1, 1));
      _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(PlayerType.Player2, 1));
      if (_eventsInitialized) return;
      _blackjackService.CardRequested += OnCardDrawRequested;
      _blackjackService.PassTurnRequested += OnPassTurnRequested;
      _blackjackService.EndTurnRequested += OnEndTurnRequested;
      _blackjackService.DealSpecificCard += OnDealSpecificCard;
      _blackjackService.CardWithSpecificValueRequested += OnCardWithSpecificValueRequested;
      _eventsInitialized = true;
   }



   private void OnCardWithSpecificValueRequested(object sender, GetCardWithSpecificValueEventArgs e)
   {
      string foundCardID = "";
      for (int i = 0; i < _blackjackService.CurrentDeck.Count; i++)
      {
         if (!string.IsNullOrEmpty(foundCardID)) break;
         var card = _blackjackService.GetCardByID(_blackjackService.CurrentDeck[i]);
         if (card == null)
         {
            Debug.LogError($"Card not found in DrawValueCardEffect.");
            continue;
         }
         var totalScoreAmount = 0f;
         for (int j = 0; j < card.DrawCardEffects.Count; j++)
         {
            if (card.DrawCardEffects[j] is AddValueEffect valueEffect) totalScoreAmount += valueEffect.cardValue;  
         }
         if (totalScoreAmount == e.ValueToDraw) foundCardID = card.CardId;
      }

      if (string.IsNullOrEmpty(foundCardID))
      {
         Debug.LogWarning($"Card with value {e.ValueToDraw} not found in deck.");
         return;
      }
      OnDealSpecificCard(this, new DealSpecificCardEventArgs(e.Player, foundCardID));
   }


   private void OnDealSpecificCard(object sender, DealSpecificCardEventArgs e)
   {            
      var cardData = CreateCardData(e.CardID);
      switch (e.Player)
      {
         case PlayerType.Player1:
            _blackjackService.FirstPlayerCards.Add(e.CardID);
            _clientCardDataFirstPlayer.Add(cardData);
            UpdateCardsObserverRpc(_clientCardDataFirstPlayer, PlayerType.Player1);
            break;
         case PlayerType.Player2:
            _blackjackService.SecondPlayerCards.Add(e.CardID);
            _clientCardDataSecondPlayer.Add(cardData);
            UpdateCardsObserverRpc(_clientCardDataSecondPlayer, PlayerType.Player2);
            break;
      }
   }

   private void OnEndTurnRequested(object sender, EndTurnRequestedEventArgs e)
   {
      switch (e.CurrentPlayer)
      {
         case PlayerType.Player1:
            if (_blackjackService.BlackjackState != BlackjackState.Player1Turn)
            {
               Debug.LogWarning($"Pass turn requested by {e.CurrentPlayer} but it is not his turn.");
               return;
            }
            _firstPlayerFinishedTurn = true;
            ChangeStateObserversRpc(BlackjackState.Player2Turn);
            CheckForRoundEnd();
            break;
         case PlayerType.Player2:
            if (_blackjackService.BlackjackState != BlackjackState.Player2Turn)
            {
               Debug.LogWarning($"Pass turn requested by {e.CurrentPlayer} but it is not his turn.");
               return;
            }

            _secondPlayerFinishedTurn = true;
            ChangeStateObserversRpc(BlackjackState.Player1Turn);
            CheckForRoundEnd();
            break;
      }
   }

   [ServerRpc(RequireOwnership = false)]
   private void CheckForRoundEnd()
   {
      if (_firstPlayerFinishedTurn && _secondPlayerFinishedTurn)
      {
         EndRoundObserversRpc();
         var result = EvaluateRoundResult();
         SendConsequencesObserversRpc(result);
         RemoveCards(_clientCardDataFirstPlayer, PlayerType.Player1);
         RemoveCards(_clientCardDataSecondPlayer, PlayerType.Player2);
         StartCoroutine(StartNextRound());
      }
   }

   private IEnumerator StartNextRound()
   {
      yield return new WaitForSeconds(5f);
      _blackjackService.CurrentDeck = _blackjackService.OrginalDeck;
      ShuffleCards();
      _firstPlayerFinishedTurn = false;
      _secondPlayerFinishedTurn = false;
      StartGameServerRpc();
   }
   

   [ServerRpc(RequireOwnership = false)]
   private void RemoveCards(List<CardClientData> clientCardData, PlayerType playerType, bool useDiscardEffect = true)
   {
      List<CardClientData> cardsRemoved = new List<CardClientData>();
      bool isCardFound = false;
      switch (playerType)
      {
         case PlayerType.Player1:
            for (int i = 0; i < clientCardData.Count; i++)
            {
               for (int j = 0; j < _clientCardDataFirstPlayer.Count; j++)
               {
                  if(clientCardData[i].CardID != _clientCardDataFirstPlayer[j].CardID) continue;
                  if (useDiscardEffect)
                     _blackjackService.OnCardPlayed(new CardPlayedEventArgs(clientCardData[i].CardID, playerType,
                        PlayType.Discard));
                  isCardFound = true;
                  break;
               }

               if (isCardFound)
               {
                  cardsRemoved.Add(clientCardData[i]);
                  _clientCardDataFirstPlayer.Remove(clientCardData[i]);
                  _blackjackService.FirstPlayerCards.Remove(clientCardData[i].CardID);
               } 
               else
                  Debug.LogWarning($"Could not find card: {clientCardData[i].CardName} for Player1");
            }
            break;
         case PlayerType.Player2: 
            for (int i = 0; i < clientCardData.Count; i++)
            {
               for (int j = 0; j < _clientCardDataSecondPlayer.Count; j++)
               {
                  if (clientCardData[i].CardID != _clientCardDataSecondPlayer[j].CardID)
                  {
                     continue;
                  }

                  if (useDiscardEffect)
                     _blackjackService.OnCardPlayed(new CardPlayedEventArgs(clientCardData[i].CardID, playerType,
                        PlayType.Discard));
                  isCardFound = true;
                  break;
               }

               if (isCardFound)
               {
                  cardsRemoved.Add(clientCardData[i]);
                  _clientCardDataSecondPlayer.Remove(clientCardData[i]);
                  _blackjackService.SecondPlayerCards.Remove(clientCardData[i].CardID);
               } 
               else
                  Debug.LogWarning($"Could not find card: {clientCardData[i].CardName} for Player2");
            }
            break;
      }
      
      RemoveCardsObserversRpc(cardsRemoved, playerType);
   }

   [ObserversRpc]
   private void RemoveCardsObserversRpc(List<CardClientData> clientCardData, PlayerType player) => _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(clientCardData , player, TransactionType.REMOVE));

   [ObserversRpc]
   private void SendConsequencesObserversRpc(RoundResult result) => _blackjackService.OnRoundConsequencesEvaluated(new RoundConsequencesEvaluatedEventArgs(result));

   [ObserversRpc]
   private void EndRoundObserversRpc()
   {
      _blackjackService.OnGameStateSet(BlackjackState.Intermission);
      Debug.Log("Round End");
   }

   private void OnPassTurnRequested(object sender, PassTurnRequestedEventArgs e)
   {
      switch (e.CurrentPlayer)
      {
         case PlayerType.Player1:
            if (_blackjackService.BlackjackState != BlackjackState.Player1Turn)
            {
               Debug.LogWarning($"Pass turn requested by {e.CurrentPlayer} but it is not his turn.");
               return;
            }
            if(!_secondPlayerFinishedTurn) ChangeStateObserversRpc(BlackjackState.Player2Turn);
            break;
         case PlayerType.Player2:
            if (_blackjackService.BlackjackState != BlackjackState.Player2Turn)
            {
               Debug.LogWarning($"Pass turn requested by {e.CurrentPlayer} but it is not his turn.");
               return;
            }
            if(!_firstPlayerFinishedTurn) ChangeStateObserversRpc(BlackjackState.Player1Turn);
            break;
      }
   }

   [ObserversRpc]
   private void ChangeStateObserversRpc(BlackjackState state)
   {
      _blackjackService.OnGameStateSet(state);
   }

   [ObserversRpc]
   private void UpdateCardsObserverRpc(List<CardClientData> cardClientDataList, PlayerType player)
   {
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(cardClientDataList, player, TransactionType.ADD));
   }

   private void OnCardDrawRequested(object sender, CardRequestedEventArgs e)
   {
      var cardID = GetFirstCardFromDeck();
      var cardSO = CreateCardData(cardID);
      if (e.PlayerType == PlayerType.Player1 && _blackjackService.BlackjackState == BlackjackState.Player1Turn)
      {
         _blackjackService.FirstPlayerCards.Add(cardID);
         _clientCardDataFirstPlayer.Add(cardSO);
         UpdateCardsObserverRpc(_clientCardDataFirstPlayer, PlayerType.Player1);
      } else if (e.PlayerType == PlayerType.Player2 && _blackjackService.BlackjackState == BlackjackState.Player2Turn)
      {
         _blackjackService.SecondPlayerCards.Add(cardID);
         _clientCardDataSecondPlayer.Add(cardSO);
         UpdateCardsObserverRpc(_clientCardDataSecondPlayer, PlayerType.Player2);
      }
   }

   private CardClientData CreateCardData(string cardId)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      var cardSO = _blackjackService.GetCardByID(cardId);
      var cardData = new CardClientData(cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath);
      return cardData;
   }
   
   private List<CardClientData> CreateCardData(List<string> cardsList)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      List<CardClientData> clientCardData = new List<CardClientData>();
      for (int i = 0; i < cardsList.Count; i++)
      {
         var cardSO = _blackjackService.GetCardByID(cardsList[i]);
         var cardData = new CardClientData(cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath);
         clientCardData.Add(cardData);
      }
      return clientCardData;
   }

   private string GetFirstCardFromDeck()
   {
      if (_blackjackService.CurrentDeck.Count <= 0)
      {
         Debug.LogError($"Deck is empty.");
         return null;
      } 
      var card = _blackjackService.CurrentDeck[0];
      _blackjackService.CurrentDeck.RemoveAt(0);
      return card;
   }


   [ObserversRpc]
   private void DealCardsObserverRpc(List<CardClientData> firstPlayerCardData, List<CardClientData> secondPlayerCardData)
   {
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(firstPlayerCardData, PlayerType.Player1, TransactionType.ADD));
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(secondPlayerCardData, PlayerType.Player2, TransactionType.ADD));
   }

   [ObserversRpc]
   private void StartGameObserverRpc(BlackjackState state) => _blackjackService.OnGameStateSet(state);

   private List<string> DealCards(int cardsCount)
   {
      List<string> drawnCards = new List<string>();
      ShuffleCards();
      for (int i = 0; i < cardsCount; i++)
      {
         if (_blackjackService.CurrentDeck.Count == 0)
         {
            //Out of cards logic
            break;
         }
         drawnCards.Add(_blackjackService.CurrentDeck[0]);
         _blackjackService.CurrentDeck.RemoveAt(0);
      }
      return drawnCards;
   }
   
   private void ShuffleCards()
   {
      for (int i = _blackjackService.CurrentDeck.Count - 1; i > 0; i--)
      {
         int j = _random.Next(i + 1);
         (_blackjackService.CurrentDeck[i], _blackjackService.CurrentDeck[j]) = (_blackjackService.CurrentDeck[j], _blackjackService.CurrentDeck[i]);
      }
   }
   
   private RoundResult EvaluateRoundResult()
   {
      var baseThreshold = _blackjackConfig.baseScoreThreshold;
      var firstPlayerScore = _blackjackService.FirstPlayerScore;
      var secondPlayerScore = _blackjackService.SecondPlayerScore;

      bool isFirstPlayerBust = firstPlayerScore > baseThreshold;
      bool isSecondPlayerBust = secondPlayerScore > baseThreshold;


      if (isFirstPlayerBust && isSecondPlayerBust)
         return RoundResult.BothPlayersLost;


      if (isFirstPlayerBust)
         return RoundResult.FirstPlayerLost;

      if (isSecondPlayerBust)
         return RoundResult.SecondPlayerLost;


      int firstPlayerDelta = baseThreshold - firstPlayerScore;
      int secondPlayerDelta = baseThreshold - secondPlayerScore;

      if (firstPlayerDelta < secondPlayerDelta)
         return RoundResult.SecondPlayerLost;

      if (secondPlayerDelta < firstPlayerDelta)
         return RoundResult.FirstPlayerLost;


      return RoundResult.Draw;
   }


   private bool AreBothPlayersConnected()
   {
      int connectedPlayers = NetworkManager.ServerManager.Clients.Count;
      return connectedPlayers >= 2;
   }

   private void OnDestroy()
   {
      if(NetworkManager != null && NetworkManager.ServerManager != null) NetworkManager.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionStateChanged;
      _blackjackService.CardRequested -= OnCardDrawRequested;
      _blackjackService.PassTurnRequested -= OnPassTurnRequested;
      _blackjackService.EndTurnRequested -= OnEndTurnRequested;
      _blackjackService.DealSpecificCard -= OnDealSpecificCard;
      _blackjackService.CardWithSpecificValueRequested -= OnCardWithSpecificValueRequested;
   }
}

public enum PlayType
{
   Draw,
   Play,
   Discard
}

public enum RoundResult
{
   FirstPlayerLost,
   SecondPlayerLost,
   BothPlayersLost,
   Draw
}
