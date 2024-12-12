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
    }

    public override void OnStartClient()
    {
        if (!IsOwner) return;
        _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        _blackjackService.CardsUpdated += OnCardsDraw;
    }



    private void OnDestroy() => _blackjackService.CardsUpdated -= OnCardsDraw;

    private void OnCardsDraw(object sender, CardsDataUpdatedEventArgs e)
    {
        if (!IsOwner) return;
        if (e.PlayerType != _playerType)
            return;
        _currentHand = e.Cards;
        foreach (var cardClientData in _currentHand) RequestCardPlay(_playerType, PlayType.Draw, cardClientData.CardID);
        
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestCardPlay(PlayerType playerType, PlayType playType, string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return;
        _blackjackService.OnCardPlayed(new CardPlayedEventArgs(cardId, playerType, playType));
    }
}
