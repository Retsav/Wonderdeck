using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using UnityEngine;
using Zenject;

public class BlackjackService : IBlackjackService
{
    public List<string> FirstPlayerCards { get; set; } = new();
    public List<string> SecondPlayerCards { get; set; } = new();
    public List<CardClientData> ClientCardDataFirstPlayerNotObfuscated { get; set; } = new();
    public List<CardClientData> ClientCardDataSecondPlayerNotObfuscated { get; set; } = new();
    public List<CardClientData> LocalFirstPlayerCards { get; set; } = new();
    public List<CardClientData> LocalSecondPlayerCards { get; set; } = new();
    public Dictionary<string, CardClientData> OrginalCardToDummy { get; set; } = new();
    public List<string> OrginalDeck { get; set; } = new();
    public List<string> CurrentDeck { get; set; } = new();
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    public int CurrentScoreThreshold { get; set; }
    public BlackjackState BlackjackState { get; set; }
    public bool FirstPlayerDrawsHidden { get; set; }
    public bool SecondPlayerDrawsHidden { get; set; }

    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdatedObserverEvent;
    public event EventHandler<GetCardWithSpecificValueEventArgs> CardWithSpecificValueRequested;
    public void OnGetCardWithSpecificValue(GetCardWithSpecificValueEventArgs args) => CardWithSpecificValueRequested?.Invoke(this, args);

    public void OnCardsUpdatedObserverEvent(CardsDataUpdatedEventArgs args) => CardsUpdatedObserverEvent?.Invoke(this, args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args) => ScoreUpdated?.Invoke(this, args);

    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args) => CardPlayed?.Invoke(this, args);
    public CardSO TryGetOriginalCard(string dummyCardID)
    {
        foreach (var cardData in OrginalCardToDummy)
        {
            if(cardData.Value.CardID != dummyCardID)
                continue;
            return GetCardByID(cardData.Key);
            /*foreach (var cardID in FirstPlayerCards)
            {
                if (cardID != orginalCardID)
                    continue;
                return GetCardByID(cardID);
            }*/
        }
        return null;
    }

    public event EventHandler RoundEndEarly;
    public event EventHandler CardEffectsResolved;
    public void OnRoundEndEarly() => RoundEndEarly?.Invoke(this, EventArgs.Empty);

    public void OnCardEffectsResolved() => CardEffectsResolved?.Invoke(this, EventArgs.Empty);

    public event EventHandler<RoundConsequencesEvaluatedEventArgs> RoundConsequencesEvaluated;
    public void OnRoundConsequencesEvaluated(RoundConsequencesEvaluatedEventArgs args) => RoundConsequencesEvaluated?.Invoke(this, args);


    private IInventoryService _inventoryService;

    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public CardSO GetCardByID(string id, NetworkConnection conn, PlayerType playerType)
    {
        var deckConfig = DebugConfigLoader.Instance.GetConfig<DeckConfig>();
        if (!conn.IsHost) return null;
        if (playerType == PlayerType.Player1)
        {
            for (int i = 0; i < FirstPlayerCards.Count; i++)
            {
                if (id == FirstPlayerCards[i])
                {
                    var card = deckConfig.cards.FirstOrDefault(x => x.CardId == id);
                    if (card != null)
                        return card;
                }
            }
        }
        for (int i = 0; i < SecondPlayerCards.Count; i++)
        {
            if (id == SecondPlayerCards[i])
            {
                var card = deckConfig.cards.FirstOrDefault(x => x.CardId == id);
                if (card != null)
                    return card;
            }
        }
        return null;
    }

    public CardSO GetCardByID(string id)
    {
        var deckConfig = DebugConfigLoader.Instance.GetConfig<DeckConfig>();
        for (int i = 0; i < deckConfig.cards.Count; i++)
        {
            if (id == deckConfig.cards[i].CardId)
                return deckConfig.cards[i];
        }
        return null;
    }

    public CardSO GetCardByIDFromDeck(string id)
    {
        if (!CurrentDeck.Contains(id))
        {
            Debug.LogError($"Card with {id} not found in Current Deck.");
            return null;
        }
        var card = GetCardByID(id);
        return card;
    }


    public Sprite GetCardFaceSprite(string id)
    {
        var card = GetCardByID(id);
        if (card == null)
            card = _inventoryService.GetItemByID(id);
        
        
        if (string.IsNullOrEmpty(card.cardFacePath))
            return null;


        string[] parts = card.cardFacePath.Split('|');
        if (parts.Length != 2)
        {
            Debug.LogWarning($"Invalid sprite path format: {card.cardFacePath}. Expected format: 'AtlasPath|SpriteName'.");
            return null;
        }

        string atlasPath = parts[0];
        string spriteName = parts[1];


        Sprite[] sprites = Resources.LoadAll<Sprite>(atlasPath);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found in atlas at path: {atlasPath}");
            return null;
        }
        
        Sprite sprite = Array.Find(sprites, s => s.name == spriteName);
        return sprite;
    }
    

    public event EventHandler<GameStateSetEventArgs> GameStateSet;
    public void OnGameStateSet(BlackjackState state)
    {
        BlackjackState = state;
        GameStateSet?.Invoke(this, new GameStateSetEventArgs(BlackjackState));
    }

    public event EventHandler<CardRequestedEventArgs> CardRequestedServer;
    public void OnCardDrawRequestedServerEvent(PlayerType playerType, HideType hideType) => CardRequestedServer?.Invoke(this, new CardRequestedEventArgs(playerType, hideType));
    public event EventHandler<CardRequestedEventArgs> CardRequestedClient;

    public void OnCardDrawRequestedClientEvent(PlayerType playerType, HideType hideType) => CardRequestedClient?.Invoke(this, new CardRequestedEventArgs(playerType, hideType));

    public event EventHandler<PassTurnRequestedEventArgs> PassTurnRequestedServer;
    public event EventHandler RoundEnd;

    public void OnRoundEnd() => RoundEnd?.Invoke(this, EventArgs.Empty);

    public void RequestPassTurnToOtherPlayer(PlayerType currentPlayer) => PassTurnRequestedServer?.Invoke(this, new PassTurnRequestedEventArgs(currentPlayer));

    public event EventHandler<EndTurnRequestedEventArgs> EndTurnRequestedServer;

    public void RequestEndTurnServer(PlayerType currentPlayer) => EndTurnRequestedServer?.Invoke(this, new EndTurnRequestedEventArgs(currentPlayer));
    public event EventHandler<EndTurnRequestedEventArgs> EndTurnRequestedClient;
    public void RequestEndTurnClient(PlayerType currentPlayer) => EndTurnRequestedClient?.Invoke(this, new EndTurnRequestedEventArgs(currentPlayer));
    public void OnRequestCardDeletion(PlayerType playerType) => RequestCardDeletion?.Invoke(this, playerType);

    public event EventHandler<PlayerType> RequestCardDeletion;
    public event EventHandler RequestCardSwap;

    public void OnRequestCardSwap() => RequestCardSwap?.Invoke(this, EventArgs.Empty);
    public event EventHandler RefreshScoreEvent;
    public void OnRefreshScore() => RefreshScoreEvent?.Invoke(this, EventArgs.Empty);
    public void ChangeDrawHidden(PlayerType playerType, PlayerFilter playerFilter, bool drawHidden)
    {
        if (playerType == PlayerType.Player1)
        {
            if (playerFilter == PlayerFilter.Opponent)
                SecondPlayerDrawsHidden = drawHidden;
            else
                FirstPlayerDrawsHidden = drawHidden;
        }
        else
        {
            if (playerFilter == PlayerFilter.Opponent)
                FirstPlayerDrawsHidden = drawHidden;
            else
                SecondPlayerDrawsHidden = drawHidden;
        }
    }

    public event EventHandler<CardVisualRequestedEventArgs> CardVisualRequested;
    public event EventHandler<int> GameScoreUpdated;
    public void OnGameScoreUpdated(int newScore) => GameScoreUpdated?.Invoke(this, newScore);
    public event Action ScoreThresholdChanged;
    public void OnScoreThresholdChanged() => ScoreThresholdChanged?.Invoke();

    public void OnCardVisualRequested(CardClientData card, PlayerType owner, TransactionType transactionType) => CardVisualRequested?.Invoke(this, new CardVisualRequestedEventArgs(card, owner, transactionType));
    public event EventHandler<RevealCardsEventArgs> RevealCardsEvent;
    public void OnRevealCards(PlayerType playerType, PlayerFilter playerFilter) => RevealCardsEvent?.Invoke(this, new RevealCardsEventArgs(playerType, playerFilter));
    public void OnRevealCardVisual(string orginalCardID, string dummyCardID, PlayerType targetedPlayer) => RevealCardsVisualEvent?.Invoke(this, new RevealCardsEventVisualArgs(orginalCardID, dummyCardID, targetedPlayer));

    public event EventHandler<RevealCardsEventVisualArgs> RevealCardsVisualEvent;
}

