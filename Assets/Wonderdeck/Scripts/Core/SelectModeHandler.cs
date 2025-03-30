using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class SelectModeHandler : NetworkBehaviour
{
    private DiContainer _container;
    private ISelectModeService _selectModeService;
    private IInventoryService _inventoryService;


    [Inject]
    private void ResolveDependencies(DiContainer container, ISelectModeService selectModeService, IInventoryService inventoryService)
    {
        _container = container;
        _selectModeService = selectModeService;
        _inventoryService = inventoryService;
    }

    private void LateUpdate()
    {
        if(!_selectModeService.IsSelectionMode)
            return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _selectModeService.CancelSelectionMode(NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2);
        }
    }

    public override void OnStartClient()
    {
        _selectModeService.RequestSelectionEffectExecution += OnRequestSelectionEffect;
        _selectModeService.ConnectionModeCanceled += OnConnectionModeCanceled;
        if(NetworkManager.ClientManager.Connection.IsHost)
            _selectModeService.RequestConnectionModeEnter += OnRequestConnectionModeEnter;
    }

    private void OnRequestConnectionModeEnter(object sender, SelectionEffectOperationEventArgs e)
    {
        EnterConnectionModeObserverRpc(e.CardOwner, e.CardID);
    }
    
    [ObserversRpc]
    private void EnterConnectionModeObserverRpc(PlayerType playerToEnter, string cardID)
    {
        var player = NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2;
        if (player != playerToEnter)
            return;
        _selectModeService.EnterSelectionMode(cardID);
    }

    private void OnConnectionModeCanceled(object sender, SelectionEffectOperationEventArgs e)
    {
        RefundItemServerRpc(e.CardOwner, e.CardID);
    }


    [ServerRpc(RequireOwnership = false)]
    private void RefundItemServerRpc(PlayerType playerType, string cardID)
    {
        var item = _inventoryService.GetItemByID(cardID);
        if (item == null) Debug.LogError("Item not found in SelectModeHandler!");
        _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(playerType, 1, item.name));
    }
    
    private void OnRequestSelectionEffect(object sender, SelectionEffectExecutionEventArgs e)
    {
        RequestSelectionEffectServerRpc(e.SelectedCardID, e.SelectingCardID, e.CardOwner);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSelectionEffectServerRpc(string cardID, string selectingCardID, PlayerType cardOwner)
    {
        var item = _inventoryService.GetItemByID(selectingCardID);
        if (item == null)
        {
            Debug.LogError($"Item in SelectModeHandler is null!");
            return;
        }

        if (item.PlayCardEffects == null || item.PlayCardEffects.Count == 0)
        {
            Debug.LogError("Item PlayCardEffects are null or empty!");
            return;
        }
        var cardEffectSO = item.PlayCardEffects[0];
        var runtimeEffectInstance = cardEffectSO.CreateEffect(_container);
        if (runtimeEffectInstance == null)
        {
            Debug.LogError("Failed to create effect instance in SelectModeHandler.");
            return;
        }
        if (runtimeEffectInstance is BaseCardEffect baseEffectInstance) baseEffectInstance.OnSelectionModeExecute(cardID, cardOwner);

    }

    private void OnDestroy()
    {
        _selectModeService.RequestSelectionEffectExecution -= OnRequestSelectionEffect;
        _selectModeService.ConnectionModeCanceled -= OnConnectionModeCanceled;
        _selectModeService.RequestConnectionModeEnter -= OnRequestConnectionModeEnter;
    }
}
