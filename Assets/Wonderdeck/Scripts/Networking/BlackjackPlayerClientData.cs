using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackPlayerClientData : NetworkBehaviour
{
    private IBlackjackService _blackjackService;
    private PlayerType _playerType;

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }
    
    public override void OnStartClient()
    {
        if (!IsOwner)
            return;
        _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        _blackjackService.LocalFirstPlayerCards = new List<CardClientData>();
        _blackjackService.LocalSecondPlayerCards = new List<CardClientData>();
        _blackjackService.CardsUpdated += OnCardsUpdated;
        _blackjackService.RoundEnd += OnRoundEnd;
    }

    private void OnRoundEnd(object sender, EventArgs e)
    {
        _blackjackService.LocalFirstPlayerCards = new List<CardClientData>();
        _blackjackService.LocalSecondPlayerCards = new List<CardClientData>();
    }

    private void OnCardsUpdated(object sender, CardsDataUpdatedEventArgs e)
    {
        var targetPlayerCards = e.PlayerType == PlayerType.Player1 ? _blackjackService.LocalFirstPlayerCards : _blackjackService.LocalSecondPlayerCards;
        if (e.TransactionType == TransactionType.ADD)
        {
            targetPlayerCards.Add(e.Card);
            if(_playerType == e.PlayerType)
                RequestCardPlay(_playerType, PlayType.Draw, e.Card.CardID);
        }
        else
        {
            if (targetPlayerCards.Count > 0)
            {
                var index = -1;
                for (int i = 0; i < targetPlayerCards.Count; i++)
                {
                    var card = targetPlayerCards[i];
                    if (card.CardID == e.Card.CardID)
                    {
                        index = targetPlayerCards.IndexOf(card);
                        break;
                    }
                }

                if (index == -1)
                {
                    Debug.LogError($"Cant find card {e.Card}");
                }
                targetPlayerCards.RemoveAt(index);
            }
        }
        _blackjackService.OnCardVisualRequested(e.Card, e.PlayerType, e.TransactionType);
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestCardPlay(PlayerType playerType, PlayType playType, string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return;
        _blackjackService.OnCardPlayed(new CardPlayedEventArgs(cardId, playerType, playType));
    }

    public override void OnStopClient()
    {
        _blackjackService.LocalFirstPlayerCards = null;
        _blackjackService.LocalSecondPlayerCards = null;
        Unsubscribe();
    }

    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        _blackjackService.CardsUpdated -= OnCardsUpdated;
        _blackjackService.RoundEnd -= OnRoundEnd;
    }
}
