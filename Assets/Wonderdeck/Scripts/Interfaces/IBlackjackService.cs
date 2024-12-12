using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface IBlackjackService
{
    public List<string> FirstPlayerCards { get; set; }
    public List<string> SecondPlayerCards { get; set; }
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    public BlackjackState BlackjackState { get; set; }
    
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args);
    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args);
    public CardSO GetCardByID(string id, NetworkConnection conn, PlayerType playerType);
    public CardSO GetCardByID(string id);
    public event EventHandler<GameStateSetEventArgs> GameStateSet;
    public void OnGameStateSet(BlackjackState state);
}

public class GameStateSetEventArgs : EventArgs
{
    public BlackjackState State { get; private set; }
    public GameStateSetEventArgs(BlackjackState state)
    {
        State = state;
    }
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
public enum BlackjackState
{
    Player1Turn,
    Player2Turn,
    Intermission
}