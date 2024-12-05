using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class PlayerHand : NetworkBehaviour
{
    private IBlackjackService _blackjackService;
    private List<CardClientData> _currentHand = new List<CardClientData>();
    private PlayerType _playerType;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
        _blackjackService.CardsUpdated += OnCardsUpdated;
    }

    public override void OnStartClient()
    {
        if (NetworkManager.ClientManager.Connection.IsHost)
            _playerType = PlayerType.Player1;
        else
            _playerType = PlayerType.Player2;
    }

    private void OnDestroy() => _blackjackService.CardsUpdated -= OnCardsUpdated;

    private void OnCardsUpdated(object sender, CardsDataUpdatedEventArgs e)
    {
        if (e.PlayerType != _playerType)
            return;
        _currentHand = e.Cards;
        Debug.Log($"OnCardsUpdated: {e.PlayerType}: Cards Args Count: {e.Cards.Count}");
        foreach (var cardClientData in _currentHand)
        {
            Debug.Log($"{e.PlayerType} Card: {cardClientData.CardName}");
        }
    }
}
