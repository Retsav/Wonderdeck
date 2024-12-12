using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using UnityEngine;

public class BlackjackService : IBlackjackService
{
    public List<string> FirstPlayerCards { get; set; }
    public List<string> SecondPlayerCards { get; set; }
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    public BlackjackState BlackjackState { get; set; }

    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args) => CardsUpdated?.Invoke(this, args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args) => ScoreUpdated?.Invoke(this, args);

    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args) => CardPlayed?.Invoke(this, args);

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

    public event EventHandler<GameStateSetEventArgs> GameStateSet;
    public void OnGameStateSet(BlackjackState state)
    {
        BlackjackState = state;
        GameStateSet?.Invoke(this, new GameStateSetEventArgs(BlackjackState));
    }
}

