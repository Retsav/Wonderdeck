using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectModeService
{
    public bool IsSelectionMode { get; set; }
    public string CardIDStartingSelection { get; set; }

    public void OnItemTokenClicked(CardSO cardSO, PlayerType playerType);
    public void EnterSelectionMode(string cardID);
    public void CancelSelectionMode(PlayerType playerType);
    public void OnRequestConnectionModeEnter(string cardID, PlayerType player);
    public void ExitSelectionMode();
    public event EventHandler<bool> SelectionModeStateChanged;
    public event EventHandler<SelectionEffectOperationEventArgs> ConnectionModeCanceled;
    public event EventHandler<SelectionEffectOperationEventArgs> RequestConnectionModeEnter;
    public event EventHandler<SelectionEffectExecutionEventArgs> RequestSelectionEffectExecution;
}

public class SelectionEffectOperationEventArgs : EventArgs
{
    public string CardID;
    public PlayerType CardOwner;

    public SelectionEffectOperationEventArgs(string cardID, PlayerType cardOwner)
    {
        CardID = cardID;
        CardOwner = cardOwner;
    }
}

public class SelectionEffectExecutionEventArgs : EventArgs
{
    public string SelectedCardID;
    public string SelectingCardID;
    public PlayerType CardOwner;

    public SelectionEffectExecutionEventArgs(string selectedCardID, string selectingCardID, PlayerType cardOwner)
    {
        SelectedCardID = selectedCardID;
        SelectingCardID = selectingCardID;
        CardOwner = cardOwner;
    }
}
