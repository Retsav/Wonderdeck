using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITutorialService
{
    public bool IsTutorial { get; set; }
    public bool HasHiddenCard { get; set; }
    public List<CardSO> FirstPlayerCards { get; set; }
    public List<CardSO> ArbiterCards { get; set; }
    public List<CardSO> FirstPlayerItems { get; set; }
    public List<CardSO> SecondPlayerItems { get; set; }
    public int TutorialStep { get; set; }
    
    public event EventHandler PlayerDrawRequested;
    public event EventHandler PlayerStandRequested;
    public event EventHandler<CardSO> ExecutedItem;
    public event EventHandler<bool> VisibilityPlayerButtonsChange;
    public event EventHandler<DialogueScriptableObject> OnDialoguePlay;
    public event EventHandler<int> DialogueFinished;
    public event EventHandler TutorialStepIncreased;
    public event EventHandler ActiveTutorialStepChanged;
    public void OnExecuteItem(CardSO item);
    public void OnPlayerDrawRequested();
    public void OnPlayerStandRequested();
    public void PlayDialogue(DialogueScriptableObject dialogue);
    public void IncreaseTutorialStep();
    public void OnActiveTutorialStepChanged();
    public void OnDialogueFinished(int tutorialStep);
    public void ChangeVisibilityPlayerButtonsUI(bool active);
    
    public event EventHandler ArbiterDrawTaken;
    
    
    
    
}
