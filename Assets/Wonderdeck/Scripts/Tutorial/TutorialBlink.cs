using System;
using System.Collections;
using System.Collections.Generic;
using Beautify.Universal;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class TutorialBlink : MonoBehaviour
{
    [SerializeField] private float duration = 4f;         
    [SerializeField] private float fadeDuration = 5f;

    private ITutorialService _tutorialService;

    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }
    
    private void Start()
    {
        BeautifySettings.settings.vignettingBlink.Override(1f);            
        BeautifySettings.settings.blurIntensity.Override(3f);
        _tutorialService.DialogueFinished += OnDialogueFinished;
    }

    private void OnDestroy()
    {
        _tutorialService.DialogueFinished -= OnDialogueFinished;
    }

    private void OnDialogueFinished(object sender, int e)
    {
        if (e == 0)
        {
            OpenEyes();
        }

        switch (_tutorialService.TutorialStep)
        {
            case 22:
                CloseEyes();
                break;
        }
    }

    private void CloseEyes()
    {
        BeautifySettings.settings.vignettingBlink.Override(0f);           
        BeautifySettings.settings.blurIntensity.Override(0f);     
        Sequence seq = DOTween.Sequence()
            .Join(DOTween.To(
                () => BeautifySettings.settings.vignettingBlink.value,
                x  => BeautifySettings.settings.vignettingBlink.Override(x),
                1f,                                       
                duration))
            .Join(DOTween.To(
                () => BeautifySettings.settings.blurIntensity.value,
                x  => BeautifySettings.settings.blurIntensity.Override(x),
                3f,                                         
                fadeDuration))
            .SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _tutorialService.IncreaseTutorialStep();
            }); 

    }


    private void OpenEyes()
    {
        BeautifySettings.settings.vignettingBlink.Override(1f);           
        BeautifySettings.settings.blurIntensity.Override(3f);     
        Sequence seq = DOTween.Sequence()
            .Join(DOTween.To(
                () => BeautifySettings.settings.vignettingBlink.value,
                x  => BeautifySettings.settings.vignettingBlink.Override(x),
                0f,                                       
                duration))
            .Join(DOTween.To(
                () => BeautifySettings.settings.blurIntensity.value,
                x  => BeautifySettings.settings.blurIntensity.Override(x),
                0f,                                         
                fadeDuration))
            .SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _tutorialService.IncreaseTutorialStep();
            }); 

    }
}
