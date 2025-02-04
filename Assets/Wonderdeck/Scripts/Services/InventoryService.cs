using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;
using Zenject;

public class InventoryService : IInventoryService
{
    public List<string> FirstPlayerItems { get; set; }
    public List<string> SecondPlayerItems { get; set; }
    public event EventHandler<RequestInventoryUsageEventArgs> RequestInventoryUsage;
    public event EventHandler<InventoryRequestedEventArgs> RequestInventory;
    public event EventHandler<ItemsDealRequestedEventArgs> ItemsDealRequested;
    public event EventHandler<ItemsDealRequestedEventArgs> ItemsVisualRequested;
    public void OnItemsVisualRequested(ItemsDealRequestedEventArgs args) => ItemsVisualRequested?.Invoke(this, args);

    public void OnItemsDealRequested(ItemsDealRequestedEventArgs args) => ItemsDealRequested?.Invoke(this, args);

    public event EventHandler InventoryRefreshed;


    private IBlackjackService _blackjackService;
    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService) => _blackjackService = blackjackService;
    public void OnInventoryRefreshed() => InventoryRefreshed?.Invoke(this, EventArgs.Empty);

    public void OnRequestInventoryUsage(RequestInventoryUsageEventArgs args) => RequestInventoryUsage?.Invoke(this, args);
    public void OnRequestInventory(InventoryRequestedEventArgs args) => RequestInventory?.Invoke(this, args);
    
    public CardSO GetItemByID(string id)
    {
        var itemConfig = DebugConfigLoader.Instance.GetConfig<ItemConfig>();
        for (int i = 0; i < itemConfig.itemCards.Count; i++)
        {
            if (id == itemConfig.itemCards[i].CardId)
                return itemConfig.itemCards[i];
        }
        return null;
    }

    public void AddItem(string itemID, PlayerType playerType)
    {
        var item = GetItemByID(itemID);
        if (item == null)
        {
            Debug.LogError($"Card with ID {itemID} is null when trying to add it as an item.");
            return;
        }

        switch (playerType)
        {
            case PlayerType.Player1:
                FirstPlayerItems.Add(itemID);
                break;
            case PlayerType.Player2:
                SecondPlayerItems.Add(itemID);
                break;
        }
        Debug.Log($"Successfully added item for {playerType}.");
    }

    public void RemoveItem(string itemID, PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                if (FirstPlayerItems.Contains(itemID))
                {
                    FirstPlayerItems.Remove(itemID);
                    Debug.Log($"{itemID} successfully removed from {playerType}");
                    break;
                }
                Debug.LogError($"Item {itemID} not found in {playerType}.");
                break;
            case PlayerType.Player2:
                if (SecondPlayerItems.Contains(itemID))
                {
                    SecondPlayerItems.Remove(itemID);
                    Debug.Log($"{itemID} successfully removed from {playerType}");
                    break;
                }
                Debug.LogError($"Item {itemID} not found in {playerType}.");
                break;
        }
    }

    public void UseItem(string itemID, PlayerType playerType)
    {
        switch (playerType)
        {
            case PlayerType.Player1:
                if (FirstPlayerItems.Contains(itemID))
                {
                    _blackjackService.OnCardPlayed(new CardPlayedEventArgs(itemID, PlayerType.Player1, PlayType.Play));
                    RemoveItem(itemID, PlayerType.Player1);
                }
                else
                    Debug.LogWarning($"First player does not have item: {itemID}");
                break;
            case PlayerType.Player2:
                if (SecondPlayerItems.Contains(itemID))
                {
                    _blackjackService.OnCardPlayed(new CardPlayedEventArgs(itemID, PlayerType.Player2, PlayType.Play));
                    RemoveItem(itemID, PlayerType.Player2);
                }
                else
                    Debug.LogWarning($"Second player does not have item: {itemID}");
                break;
        }
    }

    public List<string> RequestCurrentlyHadItems(PlayerType playerType, NetworkConnection conn)
    {
        throw new NotImplementedException();
    }
}
