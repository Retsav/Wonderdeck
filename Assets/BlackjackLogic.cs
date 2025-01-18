using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
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

   private Dictionary<string, CardClientData> OrginalCardToDummy = new Dictionary<string, CardClientData>();

   private PlayerType _playerType;

   private int firstPlayerLoses = 0;
   private int secondPlayerLoses = 0;

   private bool _firstPlayerFinishedTurn;
   private bool _secondPlayerFinishedTurn;
   private bool _eventsInitialized;
   private bool _gameStarted;
   

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
         _blackjackService.CurrentDeck = _blackjackService.OrginalDeck.ToList();
         NetworkManager.SceneManager.OnClientPresenceChangeEnd += OnClientLoadedScenes;
      }

      _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
      //StartGameServerRpc();
   }

   private void OnClientLoadedScenes(ClientPresenceChangeEventArgs obj)
   {
      if (_gameStarted) return;
      if (AreBothPlayersConnected())
      {
         StartGameServerRpc();
         _gameStarted = true;
      }
   }
   

   [ServerRpc(RequireOwnership = false)]
   private void StartGameServerRpc()
   {
      if (!AreBothPlayersConnected()) return;
      _blackjackService.FirstPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
      _blackjackService.SecondPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
      StartGameObserverRpc(BlackjackState.Player1Turn);

      //_clientCardDataFirstPlayer = CreateCardData(_blackjackService.FirstPlayerCards, PlayerType.Player1);
      //_clientCardDataSecondPlayer = CreateCardData(_blackjackService.SecondPlayerCards, PlayerType.Player2);
      
      
      _clientCardDataFirstPlayer.Add(CreateCardData(_blackjackService.FirstPlayerCards[0], PlayerType.Player1, true));
      _clientCardDataFirstPlayer.Add(CreateCardData(_blackjackService.FirstPlayerCards[1], PlayerType.Player1));
      _clientCardDataSecondPlayer.Add(CreateCardData(_blackjackService.SecondPlayerCards[0], PlayerType.Player2, true));
      _clientCardDataSecondPlayer.Add(CreateCardData(_blackjackService.SecondPlayerCards[1], PlayerType.Player2));
      //var secondPlayerHiddenCard = CreateCardData(_blackjackService.SecondPlayerCards[0], PlayerType.Player2, true);
      
      
      
      foreach (var card in _clientCardDataFirstPlayer) TransactionCardObserverRpc(card, PlayerType.Player1);
      foreach (var card in _clientCardDataSecondPlayer) TransactionCardObserverRpc(card, PlayerType.Player2);
      
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
      OnDealSpecificCard(this, new DealSpecificCardEventArgs(e.Player, foundCardID, false));
   }


   private void OnDealSpecificCard(object sender, DealSpecificCardEventArgs e)
   {            
      var cardData = CreateCardData(e.CardID, e.Player);
      var dummyCard = CreateCardData(e.CardID, e.Player, true);
      switch (e.Player)
      {
         case PlayerType.Player1:
            _blackjackService.FirstPlayerCards.Add(e.CardID);
            _clientCardDataFirstPlayer.Add(cardData);
            if(!e.HideCard)
               TransactionCardObserverRpc(cardData, PlayerType.Player1);
            else
               TransactionCardObserverRpc(dummyCard, PlayerType.Player1);
            break;
         case PlayerType.Player2:
            _blackjackService.SecondPlayerCards.Add(e.CardID);
            _clientCardDataSecondPlayer.Add(cardData);
            if(!e.HideCard)
               TransactionCardObserverRpc(cardData, PlayerType.Player2);
            else
               TransactionCardObserverRpc(dummyCard, PlayerType.Player2);
            break;
      }
      _blackjackService.CurrentDeck.Remove(e.CardID);
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
         switch (result)
         {
            case RoundResult.BothPlayersLost:
               firstPlayerLoses++;
               secondPlayerLoses++;
               break;
            case RoundResult.FirstPlayerLost:
               firstPlayerLoses++;
               break;
            case RoundResult.SecondPlayerLost:
               secondPlayerLoses++;
               break;
         }
         SendConsequencesObserversRpc(result);
         foreach (var card in _clientCardDataFirstPlayer) RemoveCard(card, PlayerType.Player1);
         foreach (var card in _clientCardDataSecondPlayer) RemoveCard(card, PlayerType.Player2);
         if (secondPlayerLoses >= 3 || firstPlayerLoses >= 3)
         {
            StartCoroutine(FinishGame());
         }
         else
         {
            StartCoroutine(StartNextRound());  
         }
      }
   }

   private IEnumerator FinishGame()
   {
      if (secondPlayerLoses >= 3 && firstPlayerLoses < 3) FirstPlayerWonObserverRpc();
      else if (firstPlayerLoses >= 3 && secondPlayerLoses < 3) SecondPlayerWonObserverRpc();
      else if (firstPlayerLoses >= 3 && secondPlayerLoses >= 3) DrawObserverRpc();
      yield return new WaitForSeconds(5f);
      NetworkManager.ServerManager.StopConnection(true);
   }

   private void DrawObserverRpc() => Debug.Log("Both players Draw!");
   
   private void SecondPlayerWonObserverRpc() => Debug.Log("Second Player Won!");

   private void FirstPlayerWonObserverRpc() => Debug.Log("First Player Won"!);

   private IEnumerator StartNextRound()
   {
      yield return new WaitForSeconds(5f);
      CleanUp();
      StartGameServerRpc();
   }

   private void CleanUp(bool fullReset = false)
   {
      _firstPlayerFinishedTurn = false;
      _secondPlayerFinishedTurn = false;
      _clientCardDataFirstPlayer.Clear();
      _clientCardDataSecondPlayer.Clear();
      OrginalCardToDummy.Clear();
      if (!NetworkManager.ClientManager.Connection.IsHost) return;
      ShuffleCards();
      _blackjackService.CurrentDeck = _blackjackService.OrginalDeck.ToList();
      _blackjackService.FirstPlayerCards.Clear();
      _blackjackService.FirstPlayerScore = 0;
      _blackjackService.SecondPlayerScore = 0;
      _blackjackService.SecondPlayerCards.Clear();
      if (fullReset)
      {
         firstPlayerLoses = 0;
         secondPlayerLoses = 0;
      }
   }

   private void OnDisable()
   {
      if (!_eventsInitialized) return;
      CleanUp(true);
      Unsubscribe();
   }

   [ServerRpc(RequireOwnership = false)]
   private void RemoveCard(CardClientData clientCardData, PlayerType playerType, bool useDiscardEffect = true)
   {
      bool isCardFound = false;
      switch (playerType)
      {
         case PlayerType.Player1:

            foreach (var t in _clientCardDataFirstPlayer)
            {
               if(clientCardData.CardID != t.CardID) continue;
               if (useDiscardEffect) _blackjackService.OnCardPlayed(new CardPlayedEventArgs(clientCardData.CardID, playerType, PlayType.Discard));
               isCardFound = true;
               break;
            }

            if (isCardFound)
            {
               _clientCardDataFirstPlayer.Remove(clientCardData);
               _blackjackService.FirstPlayerCards.Remove(clientCardData.CardID);
            } 
            else
               Debug.LogWarning($"Could not find card: {clientCardData.CardName} for Player1");
            break;
         case PlayerType.Player2: 
            for (int j = 0; j < _clientCardDataSecondPlayer.Count; j++)
            {
               if (clientCardData.CardID != _clientCardDataSecondPlayer[j].CardID)
               {
                  continue;
               }

               if (useDiscardEffect)
                  _blackjackService.OnCardPlayed(new CardPlayedEventArgs(clientCardData.CardID, playerType,
                     PlayType.Discard));
               isCardFound = true;
               break;
            }

            if (isCardFound)
            {
               _clientCardDataSecondPlayer.Remove(clientCardData);
               _blackjackService.SecondPlayerCards.Remove(clientCardData.CardID);
            } 
            else
               Debug.LogWarning($"Could not find card: {clientCardData.CardName} for Player2");
            break;
      }
      
      RemoveCardsObserversRpc(clientCardData, playerType);
   }

   [ObserversRpc]
   private void RemoveCardsObserversRpc(CardClientData clientCardData, PlayerType player)
   {
      if (clientCardData.IsHidden && player != _playerType)
      {
         GetDummyCardToRemoveServerRpc(clientCardData, player);
      }
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(clientCardData, player, TransactionType.REMOVE));
   }

   [ServerRpc(RequireOwnership = false)]
   private void GetDummyCardToRemoveServerRpc(CardClientData clientCardData, PlayerType player)
   {
      var dummyCardToRemove = OrginalCardToDummy[clientCardData.CardID];
      if (dummyCardToRemove == null)
      {
         Debug.LogError("Hidden cards desync! Have fun.");
         return;
      }
      RemoveDummyCardObserversRpc(dummyCardToRemove, player);
   }

   [ObserversRpc]
   private void RemoveDummyCardObserversRpc(CardClientData dummyCardToRemove, PlayerType player)
   {
      if (dummyCardToRemove.IsHidden && player != _playerType) _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(dummyCardToRemove, player, TransactionType.REMOVE));
   }

   [ObserversRpc]
   private void SendConsequencesObserversRpc(RoundResult result) => _blackjackService.OnRoundConsequencesEvaluated(new RoundConsequencesEvaluatedEventArgs(result));

   [ObserversRpc]
   private void EndRoundObserversRpc()
   {
      _blackjackService.OnGameStateSet(BlackjackState.Intermission);
      Debug.Log("Round End");
      _blackjackService.OnRoundEnd();
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
   private void TransactionCardObserverRpc(CardClientData cardClientData, PlayerType player)
   {
      if (player != _playerType && cardClientData.IsHidden)
      {
         var hiddenCard = new CardClientData(player);
         AddHiddenCardLinkServerRpc(hiddenCard, cardClientData);
         _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(hiddenCard, player, TransactionType.ADD));
      }
      else
      {
         _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(cardClientData, player, TransactionType.ADD));
      }
   }

   [ServerRpc(RequireOwnership = false)]
   private void AddHiddenCardLinkServerRpc(CardClientData dummyCard, CardClientData orginalCard) => OrginalCardToDummy[orginalCard.CardID] = dummyCard;
   
   

   private void OnCardDrawRequested(object sender, CardRequestedEventArgs e)
   {
      var cardID = GetFirstCardFromDeck();
      var cardClientData = CreateCardData(cardID, e.PlayerType);
      if (e.PlayerType == PlayerType.Player1 && _blackjackService.BlackjackState == BlackjackState.Player1Turn)
      {
         _blackjackService.FirstPlayerCards.Add(cardID);
         _clientCardDataFirstPlayer.Add(cardClientData);
         TransactionCardObserverRpc(cardClientData, PlayerType.Player1);
      } else if (e.PlayerType == PlayerType.Player2 && _blackjackService.BlackjackState == BlackjackState.Player2Turn)
      {
         _blackjackService.SecondPlayerCards.Add(cardID);
         _clientCardDataSecondPlayer.Add(cardClientData);
         TransactionCardObserverRpc(cardClientData, PlayerType.Player2);
      }
   }

   private CardClientData CreateCardData(string cardId, PlayerType owner, bool hideCard = false)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      var cardSO = _blackjackService.GetCardByID(cardId);
      var cardData = new CardClientData(hideCard, cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath, owner);
      return cardData;
   }
   
   private List<CardClientData> CreateCardData(List<string> cardsList, PlayerType owner, bool hideCard = false)
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
         var cardData = new CardClientData(hideCard, cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath, owner);
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

   private void OnDestroy() => Unsubscribe();
   public override void OnStopClient()
   {
      Unsubscribe();
      CleanUp(true);
   }

   private void Unsubscribe()
   {
      _blackjackService.CardRequested -= OnCardDrawRequested;
      _blackjackService.PassTurnRequested -= OnPassTurnRequested;
      _blackjackService.EndTurnRequested -= OnEndTurnRequested;
      _blackjackService.DealSpecificCard -= OnDealSpecificCard;
      _blackjackService.CardWithSpecificValueRequested -= OnCardWithSpecificValueRequested;
      NetworkManager.SceneManager.OnClientPresenceChangeEnd -= OnClientLoadedScenes;
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
