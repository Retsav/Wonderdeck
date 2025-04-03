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



    private IBlackjackService _blackjackService;
    private INetworkingService _networkingService;
    private ISelectModeService _selectModeService;

    private PlayerType _playerType;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, INetworkingService networkingService, ISelectModeService selectModeService)
    {
        _blackjackService = blackjackService;
        _networkingService = networkingService;
        _selectModeService = selectModeService;
    }
    
    
    private void Awake()
    {
        Hide();
    }


    public override void OnStartClient()
    {
        _playerType = _networkingService.GetPlayerType(NetworkManager.ClientManager.Connection);
        _blackjackService.GameStateSet += OnBlackjackStateSet;
        _selectModeService.SelectionModeStateChanged += OnSelectModeStateChanged;
    }

    private void OnSelectModeStateChanged(object sender, bool e)
    {
        if(e)
            Hide();
        else
            Show();
    }


    private void Update()
    {
        if (_selectModeService.IsSelectionMode)
            return;
        if(Input.GetKeyDown(KeyCode.Q))
            RequestDrawClicked();
        if(Input.GetKeyDown(KeyCode.E))
            RequestStandClicked();
        if (Input.GetKeyDown(KeyCode.F))
            RequestDebugDraw();
    }

    private void RequestDebugDraw()
    {
        RequestDrawServerRpc(_playerType, HideType.HideFromYourself);
        _blackjackService.OnCardDrawRequestedClientEvent(_playerType, HideType.HideFromYourself);
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
        switch (_playerType)
        {
            case PlayerType.Player1 when _blackjackService.BlackjackState != BlackjackState.Player1Turn:
            case PlayerType.Player2 when _blackjackService.BlackjackState != BlackjackState.Player2Turn:
                return;
            default:
                RequestStandServerRpc(_playerType);
                _blackjackService.RequestEndTurnClient(_playerType);
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStandServerRpc(PlayerType player)
    {
        _blackjackService.RequestEndTurnServer(player);
    }

    private void RequestDrawClicked()
    {
        switch (_playerType)
        {
            case PlayerType.Player1 when _blackjackService.BlackjackState != BlackjackState.Player1Turn:
            case PlayerType.Player2 when _blackjackService.BlackjackState != BlackjackState.Player2Turn:
                return;
            default:
                RequestDrawServerRpc(_playerType, HideType.None);
                _blackjackService.OnCardDrawRequestedClientEvent(_playerType, HideType.None);
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDrawServerRpc(PlayerType player, HideType hideType)
    {
        if(player == PlayerType.Player1 ? _blackjackService.FirstPlayerDrawsHidden : _blackjackService.SecondPlayerDrawsHidden)
            _blackjackService.OnCardDrawRequestedServerEvent(player, HideType.HideFromYourself);
        else
            _blackjackService.OnCardDrawRequestedServerEvent(player, hideType);
        _blackjackService.RequestPassTurnToOtherPlayer(player);
    }

    private void OnDestroy()
    {
        _blackjackService.GameStateSet -= OnBlackjackStateSet;
        _selectModeService.SelectionModeStateChanged -= OnSelectModeStateChanged;
    }
}
