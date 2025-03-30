using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class SelectionModeUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup selectionModeCanvasGroup;


    private ISelectModeService _selectModeService;
    
    [Inject]
    private void ResolveDependencies(ISelectModeService selectModeService)
    {
        _selectModeService = selectModeService;
    }


    private void Start()
    {
        _selectModeService.SelectionModeStateChanged += OnSelectionModeStateChanged;
        selectionModeCanvasGroup.DOFade(0f, 0f);
    }

    private void OnSelectionModeStateChanged(object sender, bool e)
    {
        if (e)
            selectionModeCanvasGroup.DOFade(1f, 0.3f);
        else
            selectionModeCanvasGroup.DOFade(0f, 0.3f);
    }
}
