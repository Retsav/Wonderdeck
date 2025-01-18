using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using UnityEngine;
using Zenject;

public class BlackjackService : IBlackjackService
{
    public List<string> FirstPlayerCards { get; set; }
    public List<string> SecondPlayerCards { get; set; }
    public List<CardClientData> LocalFirstPlayerCards { get; set; }
    public List<CardClientData> LocalSecondPlayerCards { get; set; }
    public List<string> OrginalDeck { get; set; }
    public List<string> CurrentDeck { get; set; }
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    public BlackjackState BlackjackState { get; set; }
    
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public event EventHandler<GetCardWithSpecificValueEventArgs> CardWithSpecificValueRequested;
    public void OnGetCardWithSpecificValue(GetCardWithSpecificValueEventArgs args) => CardWithSpecificValueRequested?.Invoke(this, args);

    public void OnCardsUpdated(CardsDataUpdatedEventArgs args) => CardsUpdated?.Invoke(this, args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args) => ScoreUpdated?.Invoke(this, args);

    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args) => CardPlayed?.Invoke(this, args);
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



    
    public Sprite GetCardFaceSprite(string id)
    {
        var card = GetCardByID(id);
        if (card == null)
            card = _inventoryService.GetItemByID(id);
        var cardSprite = Resources.Load<Sprite>(card.cardFacePath);
        return cardSprite;
    }

    public event EventHandler<DealSpecificCardEventArgs> DealSpecificCard;

    public void OnDealSpecificCard(string id, PlayerType playerType, bool hideCard)
    {
        if (!CurrentDeck.Contains(id))
        {
            Debug.LogError($"Card with {id} not found in Current Deck.");
            return;
        }
        DealSpecificCard?.Invoke(this, new DealSpecificCardEventArgs(playerType, id, hideCard));
    }
    

    public event EventHandler<GameStateSetEventArgs> GameStateSet;
    public void OnGameStateSet(BlackjackState state)
    {
        BlackjackState = state;
        GameStateSet?.Invoke(this, new GameStateSetEventArgs(BlackjackState));
    }

    public event EventHandler<CardRequestedEventArgs> CardRequested;
    public void OnCardDrawRequested(PlayerType playerType, bool hideCard) => CardRequested?.Invoke(this, new CardRequestedEventArgs(playerType, hideCard));
    
    public event EventHandler<PassTurnRequestedEventArgs> PassTurnRequested;
    public event EventHandler RoundEnd;

    public void OnRoundEnd() => RoundEnd?.Invoke(this, EventArgs.Empty);

    public void RequestPassTurnToOtherPlayer(PlayerType currentPlayer) => PassTurnRequested?.Invoke(this, new PassTurnRequestedEventArgs(currentPlayer));

    public event EventHandler<EndTurnRequestedEventArgs> EndTurnRequested;

    public void RequestEndTurn(PlayerType currentPlayer) => EndTurnRequested?.Invoke(this, new EndTurnRequestedEventArgs(currentPlayer));
    public event EventHandler<CardVisualRequestedEventArgs> CardVisualRequested;
    public void OnCardVisualRequested(CardClientData card, PlayerType owner, TransactionType transactionType) => CardVisualRequested?.Invoke(this, new CardVisualRequestedEventArgs(card, owner, transactionType));
}

