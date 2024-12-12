using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackScoring : NetworkBehaviour
{
    private IBlackjackService _blackjackService;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }


    private void Start()
    {
        _blackjackService.ScoreUpdated += OnScoreUpdated;
    }

    private void OnDestroy()
    {
        _blackjackService.ScoreUpdated -= OnScoreUpdated;
    }

    private void OnScoreUpdated(object sender, PlayerScoreUpdatedEventArgs e)
    {
        UpdateScoring(e.PlayerType, e.PlayerScore);
    }

    [ObserversRpc]
    private void UpdateScoring(PlayerType player, int score)
    {
        Debug.Log($"Player {player} scoring: {score}");
    }
}
