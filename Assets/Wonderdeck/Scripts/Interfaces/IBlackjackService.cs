using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface IBlackjackService
{
    public List<string> FirstPlayerCards { get; set; }
    public List<string> SecondPlayerCards { get; set; }
    public List<CardClientData> LocalFirstPlayerCards { get; set; }
    public List<CardClientData> LocalSecondPlayerCards { get; set; }
    public List<string> OrginalDeck { get; set; }
    public List<string> CurrentDeck { get; set; }
    public int FirstPlayerScore { get; set; }
    public int SecondPlayerScore { get; set; }
    public int CurrentScoreThreshold { get; set; }
    public BlackjackState BlackjackState { get; set; }
    
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public event EventHandler<GetCardWithSpecificValueEventArgs> CardWithSpecificValueRequested;
    public void OnGetCardWithSpecificValue(GetCardWithSpecificValueEventArgs args);
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args);
    public event EventHandler<PlayerScoreUpdatedEventArgs> ScoreUpdated;
    public void OnScoreUpdated(PlayerScoreUpdatedEventArgs args);
    public event EventHandler<CardPlayedEventArgs> CardPlayed;
    public void OnCardPlayed(CardPlayedEventArgs args);
    public event EventHandler CardEffectsResolved;
    public void OnCardEffectsResolved();
    public event EventHandler<RoundConsequencesEvaluatedEventArgs> RoundConsequencesEvaluated;
    public void OnRoundConsequencesEvaluated(RoundConsequencesEvaluatedEventArgs args);
    public CardSO GetCardByID(string id, NetworkConnection conn, PlayerType playerType);
    public CardSO GetCardByID(string id);
    public Sprite GetCardFaceSprite(string id);
    public event EventHandler<DealSpecificCardEventArgs> DealSpecificCard; 
    public void OnDealSpecificCard(string id, PlayerType playerType, bool hideCard);
    public event EventHandler<GameStateSetEventArgs> GameStateSet;
    public void OnGameStateSet(BlackjackState state);
    public event EventHandler<CardRequestedEventArgs> CardRequestedServer;
    public void OnCardDrawRequestedServerEvent(PlayerType playerType, bool hideCard);
    public event EventHandler<CardRequestedEventArgs> CardRequestedClient;
    public void OnCardDrawRequestedClientEvent(PlayerType playerType, bool hideCard);
    public event EventHandler<PassTurnRequestedEventArgs> PassTurnRequestedServer;
    public event EventHandler RoundEnd;
    public void OnRoundEnd();
    public void RequestPassTurnToOtherPlayer(PlayerType currentPlayer);
    public event EventHandler<EndTurnRequestedEventArgs> EndTurnRequestedServer; 
    public void RequestEndTurnServer(PlayerType currentPlayer);
    public event EventHandler<EndTurnRequestedEventArgs> EndTurnRequestedClient; 
    public void RequestEndTurnClient(PlayerType currentPlayer);
    public void OnRequestCardDeletion(PlayerType playerType);
    public event EventHandler<PlayerType> RequestCardDeletion;
    public event EventHandler RequestCardSwap;
    public void OnRequestCardSwap();
    
    public event EventHandler<CardVisualRequestedEventArgs> CardVisualRequested;
    public event EventHandler<int> GameScoreUpdated;
    public void OnGameScoreUpdated(int newScore);
    public event Action ScoreThresholdChanged;
    public void OnScoreThresholdChanged();
    public void OnCardVisualRequested(CardClientData card, PlayerType owner, TransactionType transactionType);
}

public class CardVisualRequestedEventArgs : EventArgs
{
    public CardClientData Card;
    public PlayerType Owner;
    public TransactionType Transaction;

    public CardVisualRequestedEventArgs(CardClientData card, PlayerType owner, TransactionType transactionType)
    {
        Card = card;
        Owner = owner;
        Transaction = transactionType;
    } 
}

public class GetCardWithSpecificValueEventArgs : EventArgs
{
    public float ValueToDraw;
    public PlayerType Player;

    public GetCardWithSpecificValueEventArgs(float valueToDraw, PlayerType playerType)
    {
        ValueToDraw = valueToDraw;
        Player = playerType;
    }
}

public class DealSpecificCardEventArgs : EventArgs
{
    public PlayerType Player { get; private set; }
    public string CardID { get; private set; }
    public bool HideCard;

    public DealSpecificCardEventArgs(PlayerType playerType, string cardID, bool hideCard)
    {
        Player = playerType;
        CardID = cardID;
        HideCard = hideCard;
    }
}

public class PassTurnRequestedEventArgs : EventArgs
{
    public PlayerType CurrentPlayer { get; private set; }

    public PassTurnRequestedEventArgs(PlayerType currentPlayer)
    {
        CurrentPlayer = currentPlayer;
    }
}

public class RoundConsequencesEvaluatedEventArgs : EventArgs
{
    public RoundResult Result { get; private set; }
    public int FinalFirstPlayerScore { get; private set; }
    public int FinalSecondPlayerScore { get; private set; }

    public RoundConsequencesEvaluatedEventArgs(RoundResult result, int finalFirstPlayerScore, int finalSecondPlayerScore)
    {
        Result = result;
        FinalFirstPlayerScore = finalFirstPlayerScore;
        FinalSecondPlayerScore = finalSecondPlayerScore;
    }
}


public class EndTurnRequestedEventArgs : EventArgs
{
    public PlayerType CurrentPlayer { get; private set; }

    public EndTurnRequestedEventArgs(PlayerType currentPlayer)
    {
        CurrentPlayer = currentPlayer;
    }
}

public class GameStateSetEventArgs : EventArgs
{
    public BlackjackState State { get; private set; }
    public GameStateSetEventArgs(BlackjackState state)
    {
        State = state;
    }
}

public class CardRequestedEventArgs : EventArgs
{
    public PlayerType PlayerType { get; private set; }
    public bool HideCard { get; private set; }

    public CardRequestedEventArgs(PlayerType playerType, bool hideCard)
    {
        PlayerType = playerType;
        HideCard = hideCard;
    }
}

public class CardsDataUpdatedEventArgs : EventArgs
{
    public CardClientData Card { get; private set; }
    public PlayerType PlayerType { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public CardsDataUpdatedEventArgs(CardClientData card, PlayerType playerType, TransactionType transactionType)
    {
        Card = card;
        PlayerType = playerType;
        TransactionType = transactionType;
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

public enum TransactionType
{
    ADD,
    REMOVE
}