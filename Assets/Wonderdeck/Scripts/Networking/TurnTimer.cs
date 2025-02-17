using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using Zenject;

public class TurnTimer : NetworkBehaviour
{
    [SerializeField] private GameObject timerGameObject;
    [SerializeField] private float turnDuration;
    [SerializeField] private TextMeshProUGUI firstPlayerTurnTimerLabel;
    [SerializeField] private TextMeshProUGUI secondPlayerTurnTimerLabel;
    
    
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
        {
            timerGameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
            return;
        }
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
        firstPlayerTurnTimerLabel.text = time.ToString("F0");
        secondPlayerTurnTimerLabel.text = time.ToString("F0");
    }

    private float GetRemainingTime()
    {
        double RemainingTime = remainingTime;
        return Mathf.Max((float)RemainingTime, 0f);
    }
}
