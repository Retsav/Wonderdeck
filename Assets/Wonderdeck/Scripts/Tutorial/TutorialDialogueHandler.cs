using System;
using System.Collections;
using System.Collections.Generic;
using Beautify.Universal;
using DG.Tweening;
using EasyTextEffects;
using TMPro;
using UnityEngine;
using Zenject;

public class TutorialDialogueHandler : MonoBehaviour
{
    private ITutorialService _tutorialService;
    private bool _isDialoguePlaying;

    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueLabel;
    [SerializeField] private TextEffect dialogueTextEffect;
    
    private DialogueScriptableObject currentDialogue;
    private AudioConfig _audioConfig;
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

    private void Start()
    {
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();
    }

    private void OnDestroy()
    {
        _tutorialService.OnDialoguePlay -= StartDialoguePlay;
    }

    private void StartDialoguePlay(object sender, DialogueScriptableObject e)
    {
        DOVirtual.Float(0f, 0.5f, 0.3f, v => BeautifySettings.settings.vignettingOuterRing.Override(v));
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
        TutorialAudioManager.Instance.OnPlaySoundAtPosition(TutorialPlayerInit.Instance.transform.position, _audioConfig.dialogueBoxPath);
        dialogueTextEffect.Refresh();
        dialogueIndex++;
    }

    private void FinishDialogue()
    {
        DOVirtual.Float(0.5f, 0f, 0.3f, v => BeautifySettings.settings.vignettingOuterRing.Override(v));
        Debug.Log("Dialogue Finished");
        _isDialoguePlaying = false;
        dialogueIndex = 0;
        currentDialogue = null;
        dialogueLabel.SetText("");
        dialogueTextEffect.Refresh();
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
