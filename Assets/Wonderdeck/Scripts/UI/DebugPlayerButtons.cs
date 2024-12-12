using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DebugPlayerButtons : NetworkBehaviour
{
    [SerializeField] private CanvasGroup _buttonsCanvasGroup;
    [SerializeField] private Button _drawButton;
    [SerializeField] private Button _standButton;


    private IBlackjackService _blackjackService;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }
    
    
    private void Awake()
    {
        _buttonsCanvasGroup.alpha = 0;
        _buttonsCanvasGroup.interactable = false;
        _buttonsCanvasGroup.blocksRaycasts = false;
        _drawButton.onClick.RemoveAllListeners();
        _standButton.onClick.RemoveAllListeners();
    }


    public override void OnStartClient()
    {
        if (!IsOwner) return;
        _drawButton.onClick.AddListener(RequestDrawClicked);
        _standButton.onClick.AddListener(RequestStandClicked);
    }

    private void RequestStandClicked()
    {
        
    }

    private void RequestDrawClicked()
    {
        throw new NotImplementedException();
    }
}
