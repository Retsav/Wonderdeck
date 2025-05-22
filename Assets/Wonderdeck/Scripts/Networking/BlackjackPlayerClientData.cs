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
        _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        if (!IsOwner)
            return;
        _blackjackService.LocalFirstPlayerCards = new List<CardClientData>();
        _blackjackService.LocalSecondPlayerCards = new List<CardClientData>();
        _blackjackService.CardsUpdatedObserverEvent += OnCardsUpdated;
        _blackjackService.RevealCardsVisualEvent += OnRevealCardsVisual;
        _blackjackService.RoundEnd += OnRoundEnd;
    }

    private void OnRevealCardsVisual(object sender, RevealCardsEventVisualArgs e)
    {
        SwitchLocalCardServerRpc(e.DummyCardID, e.OrginalCardID, e.TargetedPlayer, _playerType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwitchLocalCardServerRpc(string dummyCardID, string orginalCardID, PlayerType targetedPlayer, PlayerType targetingPlayer)
    {
        var playerCards = targetedPlayer == PlayerType.Player1
            ? _blackjackService.ClientCardDataFirstPlayerNotObfuscated
            : _blackjackService.ClientCardDataSecondPlayerNotObfuscated;

        CardClientData orginalClientData = null;
        for (int i = 0; i < playerCards.Count; i++)
        {
            var card = playerCards[i];
            if(card.CardID != orginalCardID)
                continue;
            orginalClientData = card;
            break;
        }

        if (orginalClientData == null)
        {
            //Debug.LogError($"Cant find card in SwitchLocalCardServerRpc, DummyID: {dummyCardID}, OrginalCardID {orginalCardID}");
            return;
        }

        SwitchLocalCardObserverRpc(orginalClientData, dummyCardID, targetedPlayer, targetingPlayer);
    }

    [ObserversRpc]
    private void SwitchLocalCardObserverRpc(CardClientData orginalClientData, string dummyCardID, PlayerType targetedPlayer, PlayerType targetingPlayer)
    {
        _playerType = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        if (targetingPlayer != _playerType)
            return;
        var playerCards = targetedPlayer == PlayerType.Player1
            ? _blackjackService.LocalFirstPlayerCards
            : _blackjackService.LocalSecondPlayerCards;
        
        
        int index = -1;
        for (int i = 0; i < playerCards.Count; i++)
        {
            var card = playerCards[i];
            if (card.CardID != dummyCardID)
                continue;
            index = playerCards.IndexOf(card);
            break;
        }
        if (index == -1)
        {
            //Debug.LogError($"Cant find card in SwitchLocalCardObserverRpc, DummyID: {dummyCardID}, OrginalCardID {orginalClientData.CardID}");
            return;
        }
        playerCards[index] = orginalClientData;
        _blackjackService.OnRefreshScore();
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
                RequestCardPlayServerRpc(_playerType, PlayType.Draw, e.Card.CardID);
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

                    if (card.IsHidden)
                    {
                        var orginalCard = _blackjackService.TryGetOriginalCard(card.CardID);
                        if (orginalCard == null)
                            continue;
                        if (orginalCard.CardId == e.Card.CardID)
                        {
                            index = targetPlayerCards.IndexOf(card);
                            break;
                        }
                    }
                    
                }
                if (index != -1) 
                    targetPlayerCards.RemoveAt(index);;
            }
        }
        _blackjackService.OnCardVisualRequested(e.Card, e.PlayerType, e.TransactionType, e.ActivateParticles);
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestCardPlayServerRpc(PlayerType playerType, PlayType playType, string cardId)
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
        _blackjackService.CardsUpdatedObserverEvent -= OnCardsUpdated;
        _blackjackService.RoundEnd -= OnRoundEnd;
    }
}
