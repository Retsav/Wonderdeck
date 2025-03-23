using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackHealthTracker : NetworkBehaviour
{

    private IHealthService _healthService;
    private IBlackjackService _blackjackService;
    
    
    [Inject]
    private void ResolveDependecies(IHealthService healthService, IBlackjackService blackjackService)
    {
        _healthService = healthService;
        _blackjackService = blackjackService;
    }
    
    
    public override void OnStartClient()
    {
        if (!NetworkManager.ClientManager.Connection.IsHost) return;
        _healthService.ApplyDamageViaResultEvent += OnApplyDamageViaResult;
    }

    private void OnApplyDamageViaResult(object sender, RoundResult e)
    {
        int firstPlayerDamage = 0;
        int secondPlayerDamage = 0;

        var firstPlayerScore = _blackjackService.FirstPlayerScore;
        var secondPlayerScore = _blackjackService.SecondPlayerScore;
        var threshold = _blackjackService.CurrentScoreThreshold;
        
        switch (e)
        {
            case RoundResult.Draw:
                firstPlayerDamage = 0;
                secondPlayerDamage = 0;
                break;

            case RoundResult.FirstPlayerLost:
                if (firstPlayerScore > threshold)
                    firstPlayerDamage = firstPlayerScore - threshold;
                else
                    firstPlayerDamage = threshold - firstPlayerScore;
                break;

            case RoundResult.SecondPlayerLost:
                if (secondPlayerScore > threshold)
                    secondPlayerDamage = secondPlayerScore - threshold;
                else
                    secondPlayerDamage = threshold - secondPlayerScore;
                break;

            case RoundResult.BothPlayersLost:

                firstPlayerDamage = firstPlayerScore - threshold;
                secondPlayerDamage = secondPlayerScore - threshold;
                break;
        }

        _healthService.FirstPlayerHealth -= firstPlayerDamage;
        _healthService.SecondPlayerHealth -= secondPlayerDamage;
        _healthService.OnDamageApplied(firstPlayerDamage, secondPlayerDamage);
    }

    private void OnDestroy()
    {
        _healthService.ApplyDamageViaResultEvent -= OnApplyDamageViaResult;
    }
}
