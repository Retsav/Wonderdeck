using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class WonderDeckCheats : NetworkBehaviour
{
    private IInventoryService _inventoryService;
    
    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CheatItemDeal(NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2, "RemoveLastCard", 5);
            CheatItemDeal(NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2, "SwapCards", 5);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheatItemDeal(PlayerType playerType, string itemName = "", int amount = 5)
    {
        _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(playerType, amount, itemName));
    }
}
