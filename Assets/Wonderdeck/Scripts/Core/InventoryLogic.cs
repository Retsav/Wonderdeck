using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;
using Random = System.Random;

public class InventoryLogic : NetworkBehaviour
{

    private IInventoryService _inventoryService;
    private static Random _random = new Random();


    private ItemConfig _itemConfig;

    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }
    
    public override void OnStartClient()
    {
        _inventoryService.FirstPlayerItems = new List<string>();
        _inventoryService.SecondPlayerItems = new List<string>();
        if (!ClientManager.Connection.IsHost) return;
        _inventoryService.RequestInventoryUsage += OnInventoryUsageRequested;
        _inventoryService.RequestInventory += OnInventoryRequested;
        _inventoryService.ItemsDealRequested += DealInventoryItems;
        _itemConfig = DebugConfigLoader.Instance.GetConfig<ItemConfig>();
    }

    private void DealInventoryItems(object sender, ItemsDealRequestedEventArgs e)
    {
        if (_itemConfig.itemCards == null || _itemConfig.itemCards.Count == 0)
        {
            Debug.LogError($"ItemCards is null or ItemCards count is 0.");
            return;
        }
        List<string> inventoryItems = new List<string>();
        for (int i = 0; i < e.Amount; i++)
        {
            int index = _random.Next(_itemConfig.itemCards.Count);
            inventoryItems.Add(_itemConfig.itemCards[index].CardId);
        }

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if(e.Player == PlayerType.Player1)
                _inventoryService.AddItem(inventoryItems[i], PlayerType.Player1);
            if (e.Player == PlayerType.Player2) AddItemObserverRpc(inventoryItems[i], PlayerType.Player2);
        }
        
    }


    private void AddItemObserverRpc(string id, PlayerType player) => _inventoryService.AddItem(id, player);


    private void OnInventoryRequested(object sender, InventoryRequestedEventArgs e)
    {
        if (e.Player == PlayerType.Player2) ChangePlayerInventoryObserverRpc(_inventoryService.SecondPlayerItems);
    }

    [ObserversRpc(ExcludeServer = true)]
    private void ChangePlayerInventoryObserverRpc(List<string> inventoryServiceSecondPlayerItems)
    {
        _inventoryService.SecondPlayerItems = inventoryServiceSecondPlayerItems;
    }


    private void OnInventoryUsageRequested(object sender, RequestInventoryUsageEventArgs e)
    {
        switch (e.Player)
        {
            case PlayerType.Player1:
                if (!_inventoryService.FirstPlayerItems.Contains(e.ItemID))
                {
                    Debug.LogError($"Player1 does not have item with id: {e.ItemID} in his inventory.");
                    return;
                }
                _inventoryService.UseItem(e.ItemID, PlayerType.Player1);
                break;
            case PlayerType.Player2:
                if (!_inventoryService.SecondPlayerItems.Contains(e.ItemID))
                {
                    Debug.LogError($"Player2 does not have item with id: {e.ItemID} in his inventory.");
                    return;
                }
                _inventoryService.UseItem(e.ItemID, PlayerType.Player2);
                ChangePlayerInventoryObserverRpc(_inventoryService.SecondPlayerItems);
                break;
        }
    }

    private void OnDestroy()
    {
        _inventoryService.RequestInventoryUsage -= OnInventoryUsageRequested;
        _inventoryService.RequestInventory -= OnInventoryRequested;
    }
}
