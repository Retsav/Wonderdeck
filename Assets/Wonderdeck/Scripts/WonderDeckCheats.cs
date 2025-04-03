using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class WonderDeckCheats : NetworkBehaviour
{
    private IInventoryService _inventoryService;
    private IEnvironmentService _environmentService;
    
    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService, IEnvironmentService environmentService)
    {
        _inventoryService = inventoryService;
        _environmentService = environmentService;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            CheatItemDeal(NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2, "RemovePersistentItem", 2);
            CheatItemDeal(NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2, "HiddenDrawingOpponent", 2);
            CheatItemDeal(NetworkManager.ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2, "RevealOpponentCards", 2);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            _environmentService.ChangeScenery("SceneryFirst");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            _environmentService.ChangeScenery("DebugScenery1");
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            _environmentService.ChangeScenery("DebugScenery2");
        }
        
        if (Input.GetKeyDown(KeyCode.F5))
        {
            _environmentService.ChangeScenery("DebugScenery3");
        }
        
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ChangeSceneryObserverRpc("SceneryFirst");
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            ChangeSceneryObserverRpc("DebugScenery1");
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ChangeSceneryObserverRpc("DebugScenery2");
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ChangeSceneryObserverRpc("DebugScenery3");
        }
    }

    [ObserversRpc]
    private void ChangeSceneryObserverRpc(string key)
    {
        _environmentService.ChangeScenery(key);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheatItemDeal(PlayerType playerType, string itemName = "", int amount = 5)
    {
        _inventoryService.OnItemsDealRequested(new ItemsDealRequestedEventArgs(playerType, amount, itemName));
    }
}
