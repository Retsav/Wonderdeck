using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class PlayerVisual : NetworkBehaviour
{
    private IBlackjackService _blackjackService;
    private PlayerType _playerType;
    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    private void Start()
    {
        _blackjackService.RoundConsequencesEvaluated += OnRoundConsequencesEvaluated;
        _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
    }

    private void OnRoundConsequencesEvaluated(object sender, RoundConsequencesEvaluatedEventArgs e)
    {
        if (!IsOwner) return;
        switch (e.Result)
        {
            case RoundResult.BothPlayersLost:
                transform.DOShakePosition(3f);
                break;
            case RoundResult.FirstPlayerLost:
                if (_playerType != PlayerType.Player1) break;
                transform.DOShakePosition(3f);
                break;
            case RoundResult.SecondPlayerLost:
                if (_playerType != PlayerType.Player2) break;
                transform.DOShakePosition(3f);
                break;
        }
    }
}
