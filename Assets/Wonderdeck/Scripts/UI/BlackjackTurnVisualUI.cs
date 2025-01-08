using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class BlackjackTurnVisualUI : NetworkBehaviour
{
    [FormerlySerializedAs("player1TurnVisual")] [SerializeField] private TextMeshProUGUI player1TurnVisualLabel;
    [FormerlySerializedAs("player2TurnVisual")] [SerializeField] private TextMeshProUGUI player2TurnVisualLabel;

    private TextMeshProUGUI _activeTextMeshPro;
    private PlayerType _playerType;

    private IBlackjackService _blackjackService;

    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }


    public override void OnStartClient()
    {
        _blackjackService.GameStateSet += OnGameStateSet;
        _playerType = ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        switch (_playerType)
        {
            case PlayerType.Player1:
                Destroy(player2TurnVisualLabel.gameObject);
                _activeTextMeshPro = player1TurnVisualLabel;
                break;
            case PlayerType.Player2:
                Destroy(player1TurnVisualLabel.gameObject);
                _activeTextMeshPro = player2TurnVisualLabel;
                break;
        }
    }




    private void OnGameStateSet(object sender, GameStateSetEventArgs e)
    {
        switch (e.State)
        {
            case BlackjackState.Intermission:
                _activeTextMeshPro.text = "WAIT...";
                break;
            case BlackjackState.Player1Turn:
                if (_playerType == PlayerType.Player1)
                    _activeTextMeshPro.text = "YOUR TURN...";
                else
                    _activeTextMeshPro.text = "OPPONENT TURN...";
                break;
            case BlackjackState.Player2Turn:
                if (_playerType == PlayerType.Player2)
                    _activeTextMeshPro.text = "YOUR TURN...";
                else
                    _activeTextMeshPro.text = "OPPONENT TURN...";
                break;
        }
    }
    
    
    private void OnDestroy()
    {
        _blackjackService.GameStateSet -= OnGameStateSet;
    }
}
