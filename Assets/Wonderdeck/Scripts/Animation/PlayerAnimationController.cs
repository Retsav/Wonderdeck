using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class PlayerAnimationController : NetworkBehaviour
{
    [SerializeField] private List<NetworkAnimator> networkAnimator;



    private PlayerType _playerType;
    private IBlackjackService _blackjackService;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }
    
    public override void OnStartClient()
    {
        _playerType = ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        if (!IsOwner)
        {
            Destroy(this);
            return;
        }

        _blackjackService.CardRequestedClient += OnCardRequestedClient;
        _blackjackService.EndTurnRequestedClient += OnEndTurnRequested;

    }

    private void OnCardRequestedClient(object sender, CardRequestedEventArgs e)
    {
        if(e.PlayerType == _playerType)
            foreach (var na in networkAnimator)
                na.SetTrigger("Draw");
            
    }

    private void OnEndTurnRequested(object sender, EndTurnRequestedEventArgs e)
    {
        if(e.CurrentPlayer == _playerType)
            foreach (var na in networkAnimator)
                na.SetTrigger("Draw");
    }

    private void OnDestroy()
    {
        _blackjackService.CardRequestedClient -= OnCardRequestedClient;
        _blackjackService.EndTurnRequestedClient -= OnEndTurnRequested;
    }
}
