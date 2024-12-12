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
   
   private HashSet<CardClientData> _clientCardDataFirstPlayer = new HashSet<CardClientData>();
   private HashSet<CardClientData> _clientCardDataSecondPlayer = new HashSet<CardClientData>();
   
   private Random _random = new Random();

   private IBlackjackService _blackjackService;


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
      if (AreBothPlayersConnected())
      {
         _blackjackService.FirstPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
         _blackjackService.SecondPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
         StartGameObserverRpc(BlackjackState.Intermission);
         _clientCardDataFirstPlayer = CreateCardData(_blackjackService.FirstPlayerCards);
         _clientCardDataSecondPlayer = CreateCardData(_blackjackService.SecondPlayerCards);
         DealCardsObserverRpc(_clientCardDataFirstPlayer.ToList(), _clientCardDataSecondPlayer.ToList());
      }
   }

   private HashSet<CardClientData> CreateCardData(List<string> cardsList)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      HashSet<CardClientData> clientCardData = new HashSet<CardClientData>();
      for (int i = 0; i < cardsList.Count; i++)
      {
         var cardSO = _blackjackService.GetCardByID(cardsList[i]);
         var cardData = new CardClientData(cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath);
         clientCardData.Add(cardData);
      }
      return clientCardData;
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
