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
    private INetworkingService _networkingService;

    private PlayerType _playerType;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, INetworkingService networkingService)
    {
        _blackjackService = blackjackService;
        _networkingService = networkingService;
    }
    
    
    private void Awake()
    {
        Hide();
        _drawButton.onClick.RemoveAllListeners();
        _standButton.onClick.RemoveAllListeners();
    }


    public override void OnStartClient()
    {
        _playerType = _networkingService.GetPlayerType(NetworkManager.ClientManager.Connection);
        _drawButton.onClick.AddListener(RequestDrawClicked);
        _standButton.onClick.AddListener(RequestStandClicked);
        _blackjackService.GameStateSet += OnBlackjackStateSet;
    }

    private void OnBlackjackStateSet(object sender, GameStateSetEventArgs e)
    {
        switch (e.State)
        {
            case BlackjackState.Player1Turn:
                if (_playerType != PlayerType.Player1)
                    Hide();
                else
                    Show();
                break;
            case BlackjackState.Player2Turn:
                if(_playerType != PlayerType.Player2)
                    Hide();
                else
                    Show();
                break;
            default:
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

    private void RequestStandClicked()
    {
        RequestStandServerRpc(_playerType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStandServerRpc(PlayerType player)
    {
        _blackjackService.RequestEndTurn(player);
    }

    private void RequestDrawClicked()
    {
        RequestDrawServerRpc(_playerType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDrawServerRpc(PlayerType player)
    {
        _blackjackService.OnCardDrawRequested(player);
        _blackjackService.RequestPassTurnToOtherPlayer(player);
    }

    private void OnDestroy()
    {
        _drawButton.onClick.RemoveAllListeners();
        _standButton.onClick.RemoveAllListeners();
        _blackjackService.GameStateSet -= OnBlackjackStateSet;
    }
}
