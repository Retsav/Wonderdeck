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
   private List<string> _orginalDeck = new List<string>();
   private List<string> _deck = new List<string>();

   private DeckConfig _deckConfig;
   private BlackjackConfig _blackjackConfig;
   
   private List<CardClientData> _clientCardDataFirstPlayer = new List<CardClientData>();
   private List<CardClientData> _clientCardDataSecondPlayer = new List<CardClientData>();
   
   private Random _random = new Random();

   private IBlackjackService _blackjackService;

   private bool _firstPlayerFinishedTurn;
   private bool _secondPlayerFinishedTurn;
   

   [Inject]
   private void ResolveDependencies(IBlackjackService blackjackService)
   {
      _blackjackService = blackjackService;
   }
   
   public override void OnStartClient()
   {
      if (NetworkManager.ClientManager.Connection.IsHost)
      {
         _deckConfig = DebugConfigLoader.Instance.GetConfig<DeckConfig>();
         _blackjackConfig = DebugConfigLoader.Instance.GetConfig<BlackjackConfig>();
         for (int i = 0; i < _deckConfig.cards.Count; i++) _orginalDeck.Add(_deckConfig.cards[i].CardId);
         _deck = _orginalDeck;
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
      _blackjackService.CardRequested += OnCardDrawRequested;
      _blackjackService.PassTurnRequested += OnPassTurnRequested;
      _blackjackService.EndTurnRequested += OnEndTurnRequested;
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
         
      }
   }

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
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(cardClientDataList, player));
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
      if (_deck.Count <= 0)
      {
         Debug.LogError($"Deck is empty.");
         return null;
      } 
      var card = _deck[0];
      _deck.RemoveAt(0);
      return card;
   }


   [ObserversRpc]
   private void DealCardsObserverRpc(List<CardClientData> firstPlayerCardData, List<CardClientData> secondPlayerCardData)
   {
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(firstPlayerCardData, PlayerType.Player1));
      _blackjackService.OnCardsUpdated(new CardsDataUpdatedEventArgs(secondPlayerCardData, PlayerType.Player2));
   }

   [ObserversRpc]
   private void StartGameObserverRpc(BlackjackState state)
   {
      _blackjackService.OnGameStateSet(state);
      Debug.Log($"Game state changed to {state}");
   }

   private List<string> DealCards(int cardsCount)
   {
      List<string> drawnCards = new List<string>();
      ShuffleCards();
      for (int i = 0; i < cardsCount; i++)
      {
         if (_deck.Count == 0)
         {
            //Out of cards logic
            break;
         }
         drawnCards.Add(_deck[0]);
         _deck.RemoveAt(0);
      }
      return drawnCards;
   }
   
   private void ShuffleCards()
   {
      for (int i = _deck.Count - 1; i > 0; i--)
      {
         int j = _random.Next(i + 1);
         (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
      }
   }


   private bool AreBothPlayersConnected()
   {
      int connectedPlayers = NetworkManager.ServerManager.Clients.Count;
      return connectedPlayers >= 2;
   }

   private void OnDestroy()
   {
      NetworkManager.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionStateChanged;
   }
}

public enum PlayType
{
   Draw,
   Play,
   Discard
}
