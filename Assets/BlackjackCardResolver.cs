using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackCardResolver : NetworkBehaviour
{
    private DiContainer _container;
    private IBlackjackService _blackjackService;
    private IInventoryService _inventoryService;
    
    
    
    [Inject]
    private void ResolveDependencies(DiContainer container, IBlackjackService blackjackService, IInventoryService inventoryService)
    {
        _container = container;
        _blackjackService = blackjackService;
        _inventoryService = inventoryService;
    }


    public override void OnStartClient()
    {
        if (!NetworkManager.ClientManager.Connection.IsHost) return;
        _blackjackService.CardPlayed += OnCardPlayed;
    }
    

    private void OnCardPlayed(object sender, CardPlayedEventArgs e)
    {
        CardSO card = _blackjackService.GetCardByID(e.CardID, NetworkManager.ClientManager.Connection, e.PlayerType);
        if (card == null) card = _inventoryService.GetItemByID(e.CardID);

        if (card == null)
        {
            return;
        }
            

        switch (e.PlayType)
        {
            case PlayType.Draw:
                ResolveEffects(card.DrawCardEffects, e.PlayerType, card.CardId);
                break;
            case PlayType.Play:
                ResolveEffects(card.PlayCardEffects, e.PlayerType, card.CardId);
                break;
            case PlayType.Discard:
                ResolveEffects(card.DiscardCardEffects, e.PlayerType, card.CardId);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        SendCardsResolvedEventObserverRpc();
    }

    [ObserversRpc]
    private void SendCardsResolvedEventObserverRpc()
    {
        _blackjackService.OnCardEffectsResolved();
    }

    private void ResolveEffects(List<CardEffectSO> effects, PlayerType playerType, string cardID)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i].CreateEffect(_container);
            effect.OnExecute(playerType, cardID);
        }
    }

    public override void OnStopClient() => _blackjackService.CardPlayed -= OnCardPlayed;

    private void OnDisable() => _blackjackService.CardPlayed -= OnCardPlayed;

    private void OnDestroy() => _blackjackService.CardPlayed -= OnCardPlayed;
}
