using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackHealthTracker : NetworkBehaviour
{
    [SerializeField] private GameObject axeGameObject;
    
    
    
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
        _blackjackService.RoundEnd += OnRoundEnd;
        if (!NetworkManager.ClientManager.Connection.IsHost) return;
        _healthService.ApplyDamageViaResultEvent += OnApplyDamageViaResult;
        _healthService.ChangeDamageModifierEvent += OnDamageModifierChanged;
    }

    private void OnRoundEnd(object sender, EventArgs e)
    {
        if (axeGameObject.activeInHierarchy)
            axeGameObject.SetActive(false);
    }

    private void OnDamageModifierChanged(object sender, ChangeDamageModifierEventArgs e)
    {
        if (e.PlayerType == PlayerType.Player1)
        {
            switch (e.Target)
            {
                case PlayerFilter.Yourself:
                    _healthService.FirstPlayerDamageModifier += e.DamageModifier;
                    break;
                case PlayerFilter.Opponent:
                    _healthService.SecondPlayerDamageModifier += e.DamageModifier;
                    break;
            }
        }
        else
        {
            switch (e.Target)
            {
                case PlayerFilter.Yourself:
                    _healthService.SecondPlayerDamageModifier += e.DamageModifier;
                    break;
                case PlayerFilter.Opponent:
                    _healthService.FirstPlayerDamageModifier += e.DamageModifier;
                    break;
            }
        }
            
        UpdateModifierDataObserverRpc(_healthService.FirstPlayerDamageModifier,
            _healthService.SecondPlayerDamageModifier);
        if (axeGameObject.activeInHierarchy)
            return;
        axeGameObject.SetActive(true);
    }

    [ObserversRpc(ExcludeServer = true)]
    private void UpdateModifierDataObserverRpc(int firstPlayerDamageModifier, int secondPlayerDamageModifier)
    {
        _healthService.FirstPlayerDamageModifier = firstPlayerDamageModifier;
        _healthService.SecondPlayerDamageModifier = secondPlayerDamageModifier;
        if (axeGameObject.activeInHierarchy)
            return;
        axeGameObject.SetActive(true);
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

        firstPlayerDamage += _healthService.FirstPlayerDamageModifier;
        secondPlayerDamage += _healthService.SecondPlayerDamageModifier;
        secondPlayerDamage = Mathf.Max(secondPlayerDamage, 0);
        firstPlayerDamage = Mathf.Max(firstPlayerDamage, 0);
        
        DealDamageObserverRpc(firstPlayerDamage, secondPlayerDamage);
    }

    [ObserversRpc]
    private void DealDamageObserverRpc(int firstPlayerDamage, int secondPlayerDamage)
    {
        _healthService.FirstPlayerHealth -= firstPlayerDamage;
        _healthService.SecondPlayerHealth -= secondPlayerDamage;
        _healthService.OnDamageApplied(firstPlayerDamage, secondPlayerDamage);
    }

    private void OnDestroy()
    {
        _healthService.ApplyDamageViaResultEvent -= OnApplyDamageViaResult;
        _healthService.ChangeDamageModifierEvent -= OnDamageModifierChanged;
    }
}
