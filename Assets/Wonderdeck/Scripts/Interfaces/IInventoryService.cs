using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface IInventoryService
{
    public List<string> FirstPlayerItems { get; set; }
    public List<string> SecondPlayerItems { get; set; }


    public event EventHandler<RequestInventoryUsageEventArgs> RequestInventoryUsage;
    public event EventHandler<InventoryRequestedEventArgs> RequestInventory;
    public event EventHandler<ItemsDealRequestedEventArgs> ItemsDealRequested;
    public event EventHandler<ItemsDealRequestedEventArgs> ItemsVisualRequested;
    public void OnItemsVisualRequested(ItemsDealRequestedEventArgs args);
    public void OnItemsDealRequested(ItemsDealRequestedEventArgs args);
    public event EventHandler InventoryRefreshed;
    public void OnInventoryRefreshed();
    public void OnRequestInventoryUsage(RequestInventoryUsageEventArgs args);
    public void OnRequestInventory(InventoryRequestedEventArgs args);
    public void AddItem(string itemID, PlayerType playerType);
    public void RemoveItem(string itemID, PlayerType playerType);
    public void UseItem(string itemID, PlayerType playerType);
    public CardSO GetItemByID(string id);
    public List<string> RequestCurrentlyHadItems(PlayerType playerType, NetworkConnection conn);
}

public class ItemsDealRequestedEventArgs : EventArgs
{
    public PlayerType Player;
    public int Amount;

    public ItemsDealRequestedEventArgs(PlayerType player, int amount)
    {
        Player = player;
        Amount = amount;
    }
}

public class RequestInventoryUsageEventArgs : EventArgs
{
    public string ItemID;
    public PlayerType Player;

    public RequestInventoryUsageEventArgs(string itemID, PlayerType player)
    {
        ItemID = itemID;
        Player = player;
    }
}

public class InventoryRequestedEventArgs : EventArgs
{
    public PlayerType Player;
    public NetworkConnection Connection;

    public InventoryRequestedEventArgs(PlayerType player, NetworkConnection connection)
    {
        Player = player;
        Connection = connection;
    }
}
