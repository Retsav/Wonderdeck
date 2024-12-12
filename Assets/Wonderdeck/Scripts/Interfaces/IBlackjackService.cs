using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface IBlackjackService
{
    public List<CardSO> FirstPlayerCards { get; set; }
    public List<CardSO> SecondPlayerCards { get; set; }
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args);
    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args);
    public CardSO GetCardByID(string id, NetworkConnection conn, PlayerType playerType);
}

public class CardsDataUpdatedEventArgs : EventArgs
{
    public List<CardClientData> Cards { get; private set; }
    public PlayerType PlayerType { get; private set; }
    public CardsDataUpdatedEventArgs(List<CardClientData> cards, PlayerType playerType)
    {
        Cards = cards;
        PlayerType = playerType;
    }
}

public class PlayerScoreUpdatedEventArgs : EventArgs
{
    public PlayerType PlayerType { get; private set; }
    public int PlayerScore { get; private set; }

    public PlayerScoreUpdatedEventArgs(PlayerType playerType, int playerScore)
    {
        PlayerType = playerType;
        PlayerScore = playerScore;
    } 
}
public class CardPlayedEventArgs : EventArgs
{
    public string CardID { get; private set; }
    public PlayerType PlayerType { get; private set; }
    public PlayType PlayType { get; private set; }

    public CardPlayedEventArgs(string cardID, PlayerType playerType, PlayType playType)
    {
        CardID = cardID;
        PlayerType = playerType;
        PlayType = playType;
    }
}

public enum PlayerType { Player1, Player2 }