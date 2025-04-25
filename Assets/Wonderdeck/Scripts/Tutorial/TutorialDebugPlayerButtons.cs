using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TutorialDebugPlayerButtons : MonoBehaviour
{
    [SerializeField] private CanvasGroup _buttonsCanvasGroup;

    public bool DrawBlocked;
    public bool StandBlocked;

    private ITutorialService _tutorialService;
    private ISelectModeService _selectModeService;
    
    
    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService, ISelectModeService selectModeService)
    {
        _tutorialService = tutorialService;
        _selectModeService = selectModeService;
    }
    
    
    private void Awake()
    {
        Hide();
        DrawBlocked = true;
        StandBlocked = true;
        _tutorialService.VisibilityPlayerButtonsChange += VisibilityButtonsChanged;
        _tutorialService.DialogueFinished += OnDialogueFinished;
        _tutorialService.ActiveTutorialStepChanged += OnActiveStepChanged;
        _selectModeService.SelectionModeStateChanged += OnSelectModeStateChanged;
    }

    

    private void OnSelectModeStateChanged(object sender, bool e)
    {
        if(e)
            Hide();
        else
            Show();
    }

    private void OnActiveStepChanged(object sender, EventArgs e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 3:
                Hide();
                DrawBlocked = true;
                StandBlocked = true;
                break;
            case 10:
                Hide();
                DrawBlocked = true;
                StandBlocked = true;
                break;
            case 17:
                Hide();
                DrawBlocked = true;
                StandBlocked = true;
                break;
            case 22:
                Hide();
                DrawBlocked = true;
                StandBlocked = true;
                break;
        }
    }

    private void OnDialogueFinished(object sender, int e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 2:
                Show();
                DrawBlocked = false;
                break;
            case 4:
                Show();
                StandBlocked = false;
                break;
            case 9:
                Show();
                break;
            case 10:
                Show();
                DrawBlocked = true;
                StandBlocked = false;
                break;
            case 16:
                Show();
                DrawBlocked = true;
                StandBlocked = false;
                break;
            case 17:
                Show();
                DrawBlocked = true;
                StandBlocked = false;
                break;
            case 21:
                Show();
                DrawBlocked = true;
                StandBlocked = true;
                break;
        }
    }

    private void VisibilityButtonsChanged(object sender, bool e)
    {
        if(e)
            Show();
        else
            Hide();
    }

    private void Update()
    {
        if (_selectModeService.IsSelectionMode)
            return;
        if(Input.GetKeyDown(KeyCode.Q))
            RequestDrawClicked();
        if(Input.GetKeyDown(KeyCode.E))
            RequestStandClicked();
    }

    private void RequestStandClicked()
    {
        if (StandBlocked)
            return;
        switch (_tutorialService.TutorialStep)
        {
            case 4:
                _tutorialService.OnPlayerStandRequested();
                DrawBlocked = true;
                StandBlocked = true;
                Hide();
                break;
            case 10:
                _tutorialService.OnPlayerStandRequested();
                StandBlocked = true;
                DrawBlocked = true;
                Hide();
                break;
            case 17:
                _tutorialService.OnPlayerStandRequested();
                StandBlocked = true;
                DrawBlocked = true;
                Hide();
                break;
        }
    }

    private void RequestDrawClicked()
    {
        if (DrawBlocked)
            return;
        switch (_tutorialService.TutorialStep)
        {
            case 2:
                _tutorialService.OnPlayerDrawRequested();
                StandBlocked = true;
                DrawBlocked = true;
                Hide();
                break;
        }
        
    }

    private void Hide()
    {
        _buttonsCanvasGroup.alpha = 0;
        _buttonsCanvasGroup.interactable = false;
        _buttonsCanvasGroup.blocksRaycasts = false;
    }
    
    private void Show()
    {
        _buttonsCanvasGroup.alpha = 1;
        _buttonsCanvasGroup.interactable = true;
        _buttonsCanvasGroup.blocksRaycasts = true;
    }

    private void OnDestroy()
    {
        _tutorialService.VisibilityPlayerButtonsChange -= VisibilityButtonsChanged;
        _tutorialService.DialogueFinished -= OnDialogueFinished;
        _tutorialService.ActiveTutorialStepChanged -= OnActiveStepChanged;
        _selectModeService.SelectionModeStateChanged -= OnSelectModeStateChanged;
    }
}
