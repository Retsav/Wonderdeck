using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlackjackService
{
    public event EventHandler<CardsDataUpdatedEventArgs> CardsUpdated;
    public void OnCardsUpdated(CardsDataUpdatedEventArgs args);
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

public enum PlayerType { Player1, Player2 }