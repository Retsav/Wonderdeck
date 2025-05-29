using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TurnTimer : NetworkBehaviour
{
    [SerializeField] private float turnDuration;
    
    [SerializeField] private Image firstPlayerTimerImage;
    [SerializeField] private Image secondPlayerTimerImage;
    
    private double turnEndTime;
    private double remainingTime;
    
    
    private Coroutine turnTimerCoroutine;
    private IBlackjackService _blackjackService;

    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    public override void OnStartClient()
    {
        if (!NetworkManager.ClientManager.Connection.IsHost)
            return;
        _blackjackService.GameStateSet += OnGameStateSet;
    }

    private void OnDestroy()
    {
        _blackjackService.GameStateSet -= OnGameStateSet;
    }

    private void OnGameStateSet(object sender, GameStateSetEventArgs e)
    {
        if (e.State == BlackjackState.Intermission)
            return;
        ResetTimer();
    }

    private void ResetTimer()
    {
        turnEndTime = InstanceFinder.TimeManager.Tick + turnDuration * InstanceFinder.TimeManager.TickRate;
        remainingTime = (turnEndTime - InstanceFinder.TimeManager.Tick) / InstanceFinder.TimeManager.TickRate;
        UpdateRemainingTimeObserverRpc(remainingTime);
        UpdateTimerUI(1f);
        if (turnTimerCoroutine != null)
        {
            
            StopCoroutine(turnTimerCoroutine);
        }
        turnTimerCoroutine = StartCoroutine(TurnTimerCoroutine());
    }

    [ObserversRpc]
    private void UpdateRemainingTimeObserverRpc(double timer) => remainingTime = timer;

    private IEnumerator TurnTimerCoroutine()
    {
        while (InstanceFinder.TimeManager.Tick < turnEndTime)
        {
            remainingTime = (turnEndTime - InstanceFinder.TimeManager.Tick) / InstanceFinder.TimeManager.TickRate;
            UpdateRemainingTimeObserverRpc(remainingTime);
            yield return null;
        }
        TimerExceeded();
    }

    private void TimerExceeded()
    {
        var player = PlayerType.Player1;
        switch (_blackjackService.BlackjackState)
        {
            case BlackjackState.Player1Turn:
                player = PlayerType.Player1;
                break;
            case BlackjackState.Player2Turn:
                player = PlayerType.Player2;
                break;
        }
        _blackjackService.RequestEndTurnServer(player);
    }


    private void Update()
    {
        if (_blackjackService.BlackjackState == BlackjackState.Intermission)
            return;
        float time = GetRemainingTime();
        float fillAmount = time / turnDuration;
        UpdateTimerUI(fillAmount);
    }

    private float GetRemainingTime()
    {
        double RemainingTime = remainingTime;
        return Mathf.Max((float)RemainingTime, 0f);
    }
    
    private void UpdateTimerUI(float fill)
    {
        if (firstPlayerTimerImage != null)
            firstPlayerTimerImage.fillAmount = fill;
        if (secondPlayerTimerImage != null)
            secondPlayerTimerImage.fillAmount = fill;
    }
}
