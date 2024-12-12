using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public class BlackjackService : IBlackjackService
{
    public List<CardSO> FirstPlayerCards { get; set; }
    public List<CardSO> SecondPlayerCards { get; set; }
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args) => CardsUpdated?.Invoke(this, args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args) => ScoreUpdated?.Invoke(this, args);

    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args) => CardPlayed?.Invoke(this, args);

    public CardSO GetCardByID(string id, NetworkConnection conn, PlayerType playerType)
    {
        Debug.Log("GetCardByID");
        if (!conn.IsHost) return null;
        int index = -1;
        if (playerType == PlayerType.Player1)
        {
            for (int i = 0; i < FirstPlayerCards.Count; i++)
            {
                if (FirstPlayerCards[i].cardId == id)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                Debug.LogError("Card not found. Desync or cheating.");
                return null;
            }
            return FirstPlayerCards[index];
        }
        for (int i = 0; i < SecondPlayerCards.Count; i++)
        {
            if (SecondPlayerCards[i].cardId == id)
            {
                    index = i;
                    break;
            }
        }
        if (index == -1)
        {
                Debug.LogError("Card not found. Desync or cheating.");
                return null;
        }
        return SecondPlayerCards[index];
    }
}

