using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialService : ITutorialService
{
    public bool IsTutorial { get; set; } = false;
    public bool HasHiddenCard { get; set; } = false;
    public List<CardSO> FirstPlayerCards { get; set; } = new();
    public List<CardSO> ArbiterCards { get; set; } = new();
    public List<CardSO> FirstPlayerItems { get; set; } = new();
    public List<CardSO> SecondPlayerItems { get; set; } = new();
    public int TutorialStep { get; set; } = 0;
    public event EventHandler PlayerDrawRequested;
    public event EventHandler PlayerStandRequested;
    public event EventHandler<CardSO> ExecutedItem;
    public event EventHandler<bool> VisibilityPlayerButtonsChange;
    public event EventHandler<DialogueScriptableObject> OnDialoguePlay;
    public event EventHandler<int> DialogueFinished;
    public event EventHandler TutorialStepIncreased;
    public event EventHandler ActiveTutorialStepChanged;
    public void OnExecuteItem(CardSO item) => ExecutedItem?.Invoke(this, item);

    public void OnPlayerDrawRequested() => PlayerDrawRequested?.Invoke(this, EventArgs.Empty);
    public void OnPlayerStandRequested() => PlayerStandRequested?.Invoke(this, EventArgs.Empty);
    public void PlayDialogue(DialogueScriptableObject text) => OnDialoguePlay?.Invoke(this, text);
    public void IncreaseTutorialStep() => TutorialStepIncreased?.Invoke(this, EventArgs.Empty);
    public void OnActiveTutorialStepChanged() => ActiveTutorialStepChanged?.Invoke(this, EventArgs.Empty);

    public void OnDialogueFinished(int tutorialStep) => DialogueFinished?.Invoke(this, tutorialStep);

    public void ChangeVisibilityPlayerButtonsUI(bool active) => VisibilityPlayerButtonsChange?.Invoke(this, active);

    public event EventHandler ArbiterDrawTaken;
}
