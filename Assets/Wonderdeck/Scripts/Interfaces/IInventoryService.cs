using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface IInventoryService
{
    public List<string> FirstPlayerItems { get; set; }
    public List<string> SecondPlayerItems { get; set; }


    public void AddItem(string itemID, PlayerType playerType);
    public void RemoveItem(string itemID, PlayerType playerType);
    public void UseItem(string itemID, NetworkConnection conn);
}
