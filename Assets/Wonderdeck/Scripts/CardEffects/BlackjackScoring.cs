using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using TMPro;
using UnityEngine;
using Zenject;

public class BlackjackScoring : NetworkBehaviour
{
    [SerializeField] private GameObject firstPlayerScoreObject;
    [SerializeField] private GameObject secondPlayerScoreObject;
    
    [SerializeField] private TextMeshProUGUI playerOneScoreLabel;
    [SerializeField] private TextMeshProUGUI playerSecondScoreLabel;
    
    
    private IBlackjackService _blackjackService;
    private INetworkingService _networkingService;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, INetworkingService networkingService)
    {
        _blackjackService = blackjackService;
        _networkingService = networkingService;
    }
    
    private void OnDestroy()
    {
        _blackjackService.ScoreUpdated -= OnScoreUpdated;
    }

    public override void OnStartClient()
    {
        _blackjackService.ScoreUpdated += OnScoreUpdated;
        if (_networkingService.GetPlayerType(NetworkManager.ClientManager.Connection) == PlayerType.Player2)
        {
            firstPlayerScoreObject.transform.Rotate(new Vector3(0f, 180f, 0f));
            secondPlayerScoreObject.transform.Rotate(new Vector3(0f, 180f, 0f));
        }

    }

    private void OnScoreUpdated(object sender, PlayerScoreUpdatedEventArgs e)
    {
        UpdateScoring(e.PlayerType, e.PlayerScore);
        
    }

    [ObserversRpc]
    private void UpdateScoring(PlayerType player, int score)
    {
        switch (player)
        {
            case PlayerType.Player1:
                playerOneScoreLabel.text = $"{score}/21";
                break;
            case PlayerType.Player2:
                playerSecondScoreLabel.text = $"{score}/21";
                break;
        }
        Debug.Log($"Player {player} scoring: {score}");
    }
}
