using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;


public class InventoryButtonUI : NetworkBehaviour
{
    public Button itemButton;
    [FormerlySerializedAs("itemSprite")] public Image itemImage;
    public string itemID;

    private IInventoryService _inventoryService;
    
    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public void Init(InventoryUI inventoryUI)
    {
        if (string.IsNullOrEmpty(itemID))
            return;
        itemButton.onClick.RemoveAllListeners();
        UnityAction action = () =>
        {
            ExecuteItem(itemID, ClientManager.Connection.IsHost ? PlayerType.Player1 : PlayerType.Player2);
            if(inventoryUI != null) inventoryUI.Close();
        };
        itemButton.onClick.AddListener(action);
    }

    
    [ServerRpc(RequireOwnership = false)]
    private void ExecuteItem(string itemId, PlayerType playerType)
    {
        _inventoryService.OnRequestInventoryUsage(new RequestInventoryUsageEventArgs(itemId, playerType));
    }

    public void Clear()
    {
        itemID = "";
        itemButton.onClick.RemoveAllListeners();
        itemImage.sprite = null;
    }
}
