using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectModeService : ISelectModeService
{
    public bool IsSelectionMode { get; set; } = false;
    public string CardIDStartingSelection { get; set; }




    public void OnItemTokenClicked(CardSO cardSO, PlayerType playerType)
    {
        RequestSelectionEffectExecution?.Invoke(this, new SelectionEffectExecutionEventArgs(cardSO.CardId, CardIDStartingSelection, playerType));
        ExitSelectionMode();
    }

    public void EnterSelectionMode(string cardID)
    {
        CardIDStartingSelection = cardID;
        IsSelectionMode = true;
        SelectionModeStateChanged?.Invoke(this, true);
    }


    public void CancelSelectionMode(PlayerType playerType)
    {
        ConnectionModeCanceled?.Invoke(this, new SelectionEffectOperationEventArgs(CardIDStartingSelection, playerType));
        ExitSelectionMode();
    }

    public void OnRequestConnectionModeEnter(string cardID, PlayerType player)
    {
        RequestConnectionModeEnter?.Invoke(this, new SelectionEffectOperationEventArgs(cardID, player));
    }

    public void ExitSelectionMode()
    {
        CardIDStartingSelection = "";
        IsSelectionMode = false;
        SelectionModeStateChanged?.Invoke(this, false);
    }
    
    

    public event EventHandler<bool> SelectionModeStateChanged;
    public event EventHandler<SelectionEffectOperationEventArgs> ConnectionModeCanceled;
    public event EventHandler<SelectionEffectOperationEventArgs> RequestConnectionModeEnter;
    public event EventHandler<SelectionEffectExecutionEventArgs> RequestSelectionEffectExecution;
}


