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
   private List<CardSO> _orginalDeck = new List<CardSO>();
   private List<CardSO> _deck = new List<CardSO>();

   private DeckConfig _deckConfig;
   private BlackjackConfig _blackjackConfig;

   private NetworkManager _networkManager;

   private List<CardSO> _firstPlayerCards = new List<CardSO>();
   private List<CardSO> _secondPlayerCards = new List<CardSO>();
   private HashSet<CardClientData> _clientCardDataFirstPlayer = new HashSet<CardClientData>();
   private HashSet<CardClientData> _clientCardDataSecondPlayer = new HashSet<CardClientData>();
   
   private BlackjackState _blackjackState;
   private Random _random = new Random();

   private IBlackjackService _blackjackService;


   [Inject]
   private void ResolveDependencies(IBlackjackService blackjackService)
   {
      _blackjackService = blackjackService;
   }
   
   public override void OnStartClient()
   {
      _networkManager = InstanceFinder.NetworkManager;
      if (_networkManager.ClientManager.Connection.IsHost)
      {
         _deckConfig = DebugConfigLoader.Instance.GetConfig<DeckConfig>();
         _blackjackConfig = DebugConfigLoader.Instance.GetConfig<BlackjackConfig>();
         _orginalDeck = _deckConfig.cards;
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
         _firstPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
         _secondPlayerCards = DealCards(_blackjackConfig.cardsToDeal);
         StartGameObserverRpc();
         _clientCardDataFirstPlayer = CreateCardData(_firstPlayerCards);
         _clientCardDataSecondPlayer = CreateCardData(_secondPlayerCards);
         DealCardsObserverRpc(_clientCardDataFirstPlayer.ToList(), _clientCardDataSecondPlayer.ToList());
         foreach (var cardData in _clientCardDataFirstPlayer)
         {
            Debug.Log($"[SERVER] First Player Card: {cardData.CardName}");
         }
         foreach (var cardData in _clientCardDataSecondPlayer)
         {
            Debug.Log($"[SERVER] Second Player Card: {cardData.CardName}");
         }
      }
   }

   private HashSet<CardClientData> CreateCardData(List<CardSO> cardsList)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      HashSet<CardClientData> clientCardData = new HashSet<CardClientData>();
      for (int i = 0; i < cardsList.Count; i++)
      {
         var card = cardsList[i];
         var cardData = new CardClientData(card.name, card.cardId, card.cardFacePath, card.cardBackPath);
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
   private void StartGameObserverRpc()
   {
      Debug.Log("Game started success!");

   }

   private List<CardSO> DealCards(int cardsCount)
   {
      List<CardSO> drawnCards = new List<CardSO>();
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
      int connectedPlayers = _networkManager.ServerManager.Clients.Count;
      return connectedPlayers >= 2;
   }

   private void OnDestroy()
   {
      if(_networkManager != null) _networkManager.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionStateChanged;
   }
}

public enum BlackjackState
{
   Player1Turn,
   Player2Turn,
   Intermission
}
