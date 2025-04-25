using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class TutorialDialogueHandler : MonoBehaviour
{
    private ITutorialService _tutorialService;
    private bool _isDialoguePlaying;

    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueLabel;
    private DialogueScriptableObject currentDialogue;
    private int dialogueIndex = 0;
    
    
    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }
    private void Awake()
    {
        _tutorialService.OnDialoguePlay += StartDialoguePlay;
        dialogueBox.SetActive(false);
    }

    private void OnDestroy()
    {
        _tutorialService.OnDialoguePlay -= StartDialoguePlay;
    }

    private void StartDialoguePlay(object sender, DialogueScriptableObject e)
    {
        dialogueBox.SetActive(true);
        currentDialogue = e;
        dialogueIndex = 0;
        _isDialoguePlaying = true;
        PlayNextMessage();
    }

    private void PlayNextMessage()
    {
        if (dialogueIndex > currentDialogue.DialogueList.Count - 1)
        {
            FinishDialogue();
            return;
        }
        dialogueLabel.SetText(currentDialogue.DialogueList[dialogueIndex]);
        dialogueIndex++;
    }

    private void FinishDialogue()
    {
        Debug.Log("Dialogue Finished");
        _isDialoguePlaying = false;
        dialogueIndex = 0;
        currentDialogue = null;
        dialogueLabel.SetText("");
        dialogueBox.SetActive(false);
        _tutorialService.OnDialogueFinished(_tutorialService.TutorialStep);
    }

    private void Update()
    {
        if (!_isDialoguePlaying)
            return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayNextMessage();
        }
    }
}
