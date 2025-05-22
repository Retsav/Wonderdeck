using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
   
   
   private Random _random = new Random();

   private IBlackjackService _blackjackService;
   private IInventoryService _inventoryService;
   private IHealthService _healthService;
   private IEnvironmentService _environmentService;
   private IConsequencesService _consequencesService;
   

   private PlayerType _playerType;
   
   

   private bool _firstPlayerFinishedTurn;
   private bool _secondPlayerFinishedTurn;
   private bool _eventsInitialized;
   private bool _gameStarted;
   

   [Inject]
   private void ResolveDependencies(IBlackjackService blackjackService, 
      IInventoryService inventoryService, IHealthService healthService, IEnvironmentService environmentService, IConsequencesService consequencesService)
   {
      _blackjackService = blackjackService;
      _inventoryService = inventoryService;
      _healthService = healthService;
      _environmentService = environmentService;
      _consequencesService = consequencesService;
   }


   

   public override void OnStartClient()
   {
      _environmentService.ChangeScenery("SceneryFirst");
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
      if (!AreBothPlayersConnected()) return;
      StartGameServerRpc();
      _gameStarted = true;
   }
   

   [ServerRpc(RequireOwnership = false)]
   private void StartGameServerRpc()
   {
      if (!AreBothPlayersConnected()) return;
      
      _blackjackService.CurrentScoreThreshold = _blackjackConfig.baseScoreThreshold;
      StartGameObserverRpc(BlackjackState.Player1Turn);

      var consequenceTierOnFirstPlayer = _consequencesService.GetHighestConsequenceTierOnPlayer(PlayerType.Player1);
      var consequenceTierOnSecondPlayer = _consequencesService.GetHighestConsequenceTierOnPlayer(PlayerType.Player2);
      
      HandleAddCardOperationServer(PlayerType.Player1, _blackjackConfig.cardsToDeal - 1, consequenceTierOnFirstPlayer == -1 ? HideType.None : HideType.HideFromYourself);
      HandleAddCardOperationServer(PlayerType.Player2, _blackjackConfig.cardsToDeal - 1, consequenceTierOnSecondPlayer == -1 ? HideType.None : HideType.HideFromYourself);
      HandleAddCardOperationServer(PlayerType.Player1, 1, HideType.HideFromOpponent);
      HandleAddCardOperationServer(PlayerType.Player2, 1, HideType.HideFromOpponent);
      
      _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(PlayerType.Player1, 2));
      _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(PlayerType.Player2, 2));
      if (_eventsInitialized) return;
      _blackjackService.CardRequestedServer += CardDrawRequestedServer;
      _blackjackService.PassTurnRequestedServer += PassTurnRequestedServer;
      _blackjackService.EndTurnRequestedServer += OnEndTurnRequested;
      _blackjackService.CardWithSpecificValueRequested += OnCardWithSpecificValueRequested;
      _blackjackService.GameScoreUpdated += OnGameScoreUpdated;
      _blackjackService.RequestCardDeletion += OnRequestCardDeletion;
      _blackjackService.RequestCardSwap += OnRequestCardSwapServerRpc;
      _blackjackService.RevealCardsEvent += OnRevealCardServer;
      _eventsInitialized = true;
   }

   private void OnRevealCardServer(object sender, RevealCardsEventArgs e)
   {
      var myCards = e.PlayerType == PlayerType.Player1
         ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated
         : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;
      var opponentCards = e.PlayerType == PlayerType.Player1
         ? _blackjackService.ClientCardDataSecondPlayerNotObfuscated
         : _blackjackService.ClientCardDataFirstPlayerNotObfuscated;

      var targetedCardDatas = e.PlayerFilter == PlayerFilter.Opponent ? opponentCards : myCards;
      
      PlayerType expectedOwner = e.PlayerFilter == PlayerFilter.Opponent
         ? e.PlayerType.GetOppositeType()
         : e.PlayerType;
      
      foreach (var originalCardData in targetedCardDatas)
      {
         var dummyCard = TryGetDummyCardFromOrginalID(originalCardData.CardID);
         if (dummyCard == null)
            continue;
         Debug.Log("Dummy card found in reveal cards! Checking owner..");
         if (dummyCard.Owner != expectedOwner)
            continue;
         Debug.Log("Its correct player!");
         RevealCardObserverRpc(e.PlayerType, expectedOwner, originalCardData.CardID, dummyCard.CardID);
      }
   }


   [ObserversRpc]
   private void RevealCardObserverRpc(PlayerType targetingPlayer, PlayerType targetedPlayer, string orginalCardID, string dummyCardID)
   {
      if (targetingPlayer != _playerType)
         return;
      _blackjackService.OnRevealCardVisual(orginalCardID, dummyCardID, targetedPlayer);
   }
   

   private void HandleRemoveCardOperationServer(CardClientData cardData, PlayerType playerType, bool useDiscardEffect)
   {
      CardClientData cardClientData = null;
      var playerDatas = playerType == PlayerType.Player1 ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;
      var playerCards = playerType == PlayerType.Player1
         ? _blackjackService.FirstPlayerCards
         : _blackjackService.SecondPlayerCards;
      foreach (var data in playerDatas)
      {
         if (cardData.CardID != data.CardID) continue;
         if (useDiscardEffect)
            _blackjackService.OnCardPlayed(new CardPlayedEventArgs(cardData.CardID, playerType, PlayType.Discard));
         cardClientData = data;
      }
      if (cardClientData == null)
      {
         Debug.LogWarning($"Not found {cardData.CardID} for removal!");
         return;
      }
      playerDatas.Remove(cardClientData);
      playerCards.Remove(cardClientData.CardID);
      var dummyCard = TryGetDummyCardFromOrginalID(cardClientData.CardID);
      if (dummyCard != null)
      {
         Debug.Log($"Dummy card found! Attempting to delete..");
         RemoveCardsObserversRpc(dummyCard, playerType);
         _blackjackService.OrginalCardToDummy.Remove(cardClientData.CardID);
      }
      RemoveCardsObserversRpc(cardClientData, playerType);
   }

   public CardClientData TryGetDummyCardFromOrginalID(string orginalCardID)
   {
      if (_blackjackService.OrginalCardToDummy.TryGetValue(orginalCardID, out var cardData))
      {
         return cardData;
      }
      return null;
   }
   

   [ServerRpc(RequireOwnership = false)]
   private void HandleAddCardOperationServer(PlayerType playerType, int count, HideType hideType)
   {
      var playerCards = (playerType == PlayerType.Player1)
         ? _blackjackService.FirstPlayerCards
         : _blackjackService.SecondPlayerCards;

      var clientCardDatas = (playerType == PlayerType.Player1)
         ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated
         : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;
      
      var cardIDs = DealCards(count);
      for (int i = 0; i < cardIDs.Count; i++) playerCards.Add(cardIDs[i]);

      List<CardClientData> createdCardDatas = new();
      for (int i = 0; i < cardIDs.Count; i++)
      {
         var cardID = cardIDs[i];
         var orginalCardData = CreateCardData(cardID, playerType);
         createdCardDatas.Add(orginalCardData);
         clientCardDatas.Add(orginalCardData);
      }

      List<CardUpdateEventData> updateEventDatas = new();
      
      switch (hideType)
      {
         case HideType.None:
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType));
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType.GetOppositeType()));
            }
            break;
         case HideType.HideFromOpponent: 
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               var fakeCardData = CreateFakeCardData(cardData.CardID, playerType);
               _blackjackService.OrginalCardToDummy.Add(cardData.CardID, fakeCardData);
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType, true));
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType.GetOppositeType()));
            }
            break;
         case HideType.HideFromYourself:
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               var fakeCardData = CreateFakeCardData(cardData.CardID, playerType);
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType));
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType.GetOppositeType(), true));
               _blackjackService.OrginalCardToDummy.Add(cardData.CardID, fakeCardData);
            }
            break;
         case HideType.HideFromBothPlayers:
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               var fakeCardData = CreateFakeCardData(cardData.CardID, playerType);
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType));
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType.GetOppositeType()));
               _blackjackService.OrginalCardToDummy.Add(cardData.CardID, fakeCardData);
            }
            break;
      }
      AddCardObserverRpc(updateEventDatas);
   }



   private void HandleAddCardOperationServer(PlayerType playerType, string cardID, HideType hideType, bool fromDeck = true)
   {
      CardSO card = null;
      if (fromDeck)
         card = _blackjackService.GetCardByIDFromDeck(cardID);
      else
         card = _blackjackService.GetCardByID(cardID);
      if (card == null)
         return;
      
      
      var playerCards = (playerType == PlayerType.Player1)
         ? _blackjackService.FirstPlayerCards
         : _blackjackService.SecondPlayerCards;

      var clientCardDatas = (playerType == PlayerType.Player1)
         ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated
         : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;
      
      
      playerCards.Add(cardID);

      List<CardClientData> createdCardDatas = new();
      var orginalCardData = CreateCardData(cardID, playerType);
      createdCardDatas.Add(orginalCardData);
      clientCardDatas.Add(orginalCardData);
      
      List<CardUpdateEventData> updateEventDatas = new();
      
      switch (hideType)
      {
         case HideType.None:
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType));
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType.GetOppositeType()));
            }
            break;
         case HideType.HideFromOpponent: 
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               var fakeCardData = CreateFakeCardData(cardData.CardID, playerType);
               _blackjackService.OrginalCardToDummy.Add(cardData.CardID, fakeCardData);
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType, true));
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType.GetOppositeType()));
            }
            break;
         case HideType.HideFromYourself:
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               var fakeCardData = CreateFakeCardData(cardData.CardID, playerType);
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType));
               updateEventDatas.Add(new CardUpdateEventData(cardData, playerType, playerType.GetOppositeType(), true));
               _blackjackService.OrginalCardToDummy.Add(cardData.CardID, fakeCardData);
            }
            break;
         case HideType.HideFromBothPlayers:
            for (int i = 0; i < createdCardDatas.Count; i++)
            {
               var cardData = createdCardDatas[i];
               var fakeCardData = CreateFakeCardData(cardData.CardID, playerType);
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType));
               updateEventDatas.Add(new CardUpdateEventData(fakeCardData, playerType, playerType.GetOppositeType()));
               _blackjackService.OrginalCardToDummy.Add(cardData.CardID, fakeCardData);
            }
            break;
      }
      AddCardObserverRpc(updateEventDatas);
      _blackjackService.CurrentDeck.Remove(cardID);
   }

   [ServerRpc(RequireOwnership = false)]
   private void OnRequestCardSwapServerRpc(object sender, EventArgs e)
   {
      if (_blackjackService.ClientCardDataFirstPlayerNotObfuscated.Count <= 0 || _blackjackService.ClientCardDataSecondPlayerNotObfuscated.Count <= 0)
         return;
      var firstPlayerCard = _blackjackService.ClientCardDataFirstPlayerNotObfuscated[^1];
      var secondPlayerCard = _blackjackService.ClientCardDataSecondPlayerNotObfuscated[^1];
      
      HandleRemoveCardOperationServer(firstPlayerCard, PlayerType.Player1, true);
      HandleRemoveCardOperationServer(secondPlayerCard, PlayerType.Player2, true);
      HandleAddCardOperationServer(PlayerType.Player1, secondPlayerCard.CardID, HideType.None, false);
      HandleAddCardOperationServer(PlayerType.Player2, firstPlayerCard.CardID, HideType.None, false);
   }

   private void OnRequestCardDeletion(object sender, PlayerType playerRequestingDeletion)
   {
      var playerCards = playerRequestingDeletion == PlayerType.Player1
         ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated
         : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;
      if (playerCards.Count <= 0)
         return;
      var lastCard = playerCards[^1];
      HandleRemoveCardOperationServer(lastCard, playerRequestingDeletion, true);
   }

   private void OnGameScoreUpdated(object sender, int e)
   {
      _blackjackService.CurrentScoreThreshold = e;
      OnGameScoreUpdatedObserverRpc();
   }

   [ObserversRpc]
   private void OnGameScoreUpdatedObserverRpc() => _blackjackService.OnScoreThresholdChanged();


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
            if (card.DrawCardEffects[j] is AddValueEffectSO valueEffect) totalScoreAmount += valueEffect.cardValue;
         if (totalScoreAmount == e.ValueToDraw) foundCardID = card.CardId;
      }

      if (string.IsNullOrEmpty(foundCardID))
      {
         Debug.LogWarning($"Card with value {e.ValueToDraw} not found in deck.");
         return;
      }
      HandleAddCardOperationServer(e.Player, foundCardID, HideType.None);
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
      if (!_firstPlayerFinishedTurn || !_secondPlayerFinishedTurn) return;
      _blackjackService.OnRoundEndEarly();
      EndRoundObserversRpc();
      var result = EvaluateRoundResult();
      _healthService.ApplyDamage(result);
      SendConsequencesObserversRpc(result, _blackjackService.FirstPlayerScore, _blackjackService.SecondPlayerScore);
      foreach (var card in _blackjackService.ClientCardDataFirstPlayerNotObfuscated.ToList())
      {
         HandleRemoveCardOperationServer(card, PlayerType.Player1, true);
      }

      foreach (var card in _blackjackService.ClientCardDataSecondPlayerNotObfuscated.ToList())
      {
         HandleRemoveCardOperationServer(card, PlayerType.Player2, true);
      }
      if (_healthService.FirstPlayerHealth <= 0 || _healthService.SecondPlayerHealth <= 0)
         StartCoroutine(FinishGame());
      else
         StartCoroutine(StartNextRound());
   }
   
   
   private IEnumerator FinishGame()
   {
      if (_healthService.SecondPlayerHealth <= 0 && _healthService.FirstPlayerHealth > 1) FirstPlayerWonObserverRpc();
      else if (_healthService.FirstPlayerHealth <= 0 && _healthService.SecondPlayerHealth > 1) SecondPlayerWonObserverRpc();
      else if (_healthService.SecondPlayerHealth <= 0 && _healthService.FirstPlayerHealth <= 0) DrawObserverRpc();
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
      _blackjackService.ClientCardDataFirstPlayerNotObfuscated.Clear();
      _blackjackService.ClientCardDataSecondPlayerNotObfuscated.Clear();
      _blackjackService.FirstPlayerDrawsHidden = false;
      _blackjackService.SecondPlayerDrawsHidden = false;
      _healthService.FirstPlayerDamageModifier = 0;
      _healthService.SecondPlayerDamageModifier = 0;
      _blackjackService.OrginalCardToDummy.Clear();
      if (!NetworkManager.ClientManager.Connection.IsHost) return;
      ShuffleCards();
      _blackjackService.CurrentScoreThreshold = _blackjackConfig.baseScoreThreshold;
      _blackjackService.CurrentDeck = _blackjackService.OrginalDeck.ToList();
      _blackjackService.FirstPlayerCards.Clear();
      _blackjackService.FirstPlayerScore = 0;
      _blackjackService.SecondPlayerScore = 0;
      _blackjackService.SecondPlayerCards.Clear();
      if (fullReset)
      {
         _healthService.FirstPlayerHealth = _healthService.MaxHealth;
         _healthService.SecondPlayerHealth = _healthService.MaxHealth;
      }
   }

   private void OnDisable()
   {
      if (!_eventsInitialized) return;
      CleanUp(true);
      Unsubscribe();
   }
   
   [ObserversRpc]
   private void RemoveCardsObserversRpc(CardClientData clientCardData, PlayerType player)
   {
      _blackjackService.OnCardsUpdatedObserverEvent(new CardsDataUpdatedEventArgs(clientCardData, player, TransactionType.REMOVE));
   }
   

   private CardClientData GetDummyCard(CardClientData clientData)
   {
      var dummyCardToRemove = _blackjackService.OrginalCardToDummy[clientData.CardID];
      if (dummyCardToRemove == null)
      {
         Debug.LogError("Hidden cards desync! Have fun.");
         return null;
      }
      return dummyCardToRemove;
   }
   
   [ObserversRpc]
   private void SendConsequencesObserversRpc(RoundResult result, int firstPlayerScore, int secondPlayerScore)
   {
      _blackjackService.OnRoundConsequencesEvaluated(new RoundConsequencesEvaluatedEventArgs(result, firstPlayerScore, secondPlayerScore));
   }

   [ObserversRpc]
   private void EndRoundObserversRpc()
   {
      _blackjackService.OnGameStateSet(BlackjackState.Intermission);
      Debug.Log("Round End");
      _blackjackService.OnRoundEnd();
   }

   private void PassTurnRequestedServer(object sender, PassTurnRequestedEventArgs e)
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
   private void AddCardObserverRpc(List<CardUpdateEventData> updateEvents)
   {
      foreach (var updateEvent in updateEvents)
      {
         if (updateEvent.cardReceiver != _playerType)
            continue;
         _blackjackService.OnCardsUpdatedObserverEvent(new CardsDataUpdatedEventArgs(updateEvent.cardClientData, updateEvent.cardOwner, TransactionType.ADD, updateEvent.activateParticles));
      }

   }
   

   private void CardDrawRequestedServer(object sender, CardRequestedEventArgs e)
   {
      if (e.PlayerType == PlayerType.Player1 && _blackjackService.BlackjackState == BlackjackState.Player1Turn)
      {
         HandleAddCardOperationServer(e.PlayerType, 1, e.HideType);
      } else if (e.PlayerType == PlayerType.Player2 && _blackjackService.BlackjackState == BlackjackState.Player2Turn)
      {
         HandleAddCardOperationServer(e.PlayerType, 1, e.HideType);
      }
   }

   private CardClientData CreateCardData(string cardId, PlayerType owner)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      var cardSO = _blackjackService.GetCardByID(cardId);
      var cardData = new CardClientData(false, cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath, owner);
      return cardData;
   }

   private CardClientData CreateFakeCardData(string cardId, PlayerType owner)
   {
      if (!IsServerInitialized)
      {
         Debug.LogError($"Server is not initialized when creating cards data.");
         return null;
      }
      var cardData = new CardClientData(owner);
      return cardData;
   }
   
   private List<CardClientData> CreateCardData(List<string> cardsList, PlayerType owner)
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
         var cardData = new CardClientData(false, cardSO.name, cardSO.CardId, cardSO.cardFacePath, cardSO.cardBackPath, owner);
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
   private void StartGameObserverRpc(BlackjackState state)
   {
      _healthService.FirstPlayerDamageModifier = 0;
      _healthService.SecondPlayerDamageModifier = 0;
      _blackjackService.OnGameStateSet(state);
   }

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
      var baseThreshold = _blackjackService.CurrentScoreThreshold;
      _blackjackService.FirstPlayerScore = CalculatePlayerScore(PlayerType.Player1);
      _blackjackService.SecondPlayerScore = CalculatePlayerScore(PlayerType.Player2);
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


   private int CalculatePlayerScore(PlayerType player)
   {
      float score = 0;
      var targetedCards = player == PlayerType.Player1
         ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated
         : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;
      for (int i = 0; i < targetedCards.Count; i++)
      {
         var card = targetedCards[i];
         var cardSO = _blackjackService.GetCardByID(card.CardID);
         if (cardSO == null) continue;
         foreach (var cardEffect in cardSO.DrawCardEffects)
            if (cardEffect is AddValueEffectSO effect) score += effect.cardValue;
      }
      return (int)score;
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
      _blackjackService.CardRequestedServer -= CardDrawRequestedServer;
      _blackjackService.PassTurnRequestedServer -= PassTurnRequestedServer;
      _blackjackService.EndTurnRequestedServer -= OnEndTurnRequested;
      _blackjackService.CardWithSpecificValueRequested -= OnCardWithSpecificValueRequested;
      NetworkManager.SceneManager.OnClientPresenceChangeEnd -= OnClientLoadedScenes;
      _blackjackService.RequestCardDeletion -= OnRequestCardDeletion;
      _blackjackService.RequestCardSwap -= OnRequestCardSwapServerRpc;
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

public enum HideType
{
   None,
   HideFromYourself,
   HideFromOpponent,
   HideFromBothPlayers,
}