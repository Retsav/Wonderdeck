using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class InventoryButtonUI : NetworkBehaviour
{
    public Button itemButton;
    [FormerlySerializedAs("itemSprite")] public Image itemImage;
    public string itemID;

    private IInventoryService _inventoryService;
    
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public void Init()
    {
        if (string.IsNullOrEmpty(itemID))
            return;
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(ExecuteItem);
    }

    private void ExecuteItem() => _inventoryService.UseItem(itemID, NetworkManager.ClientManager.Connection);

    public void Clear()
    {
        itemID = "";
        itemButton.onClick.RemoveAllListeners();
        itemImage.sprite = null;
    }
}
