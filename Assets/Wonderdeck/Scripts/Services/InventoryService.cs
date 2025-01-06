using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;
using Zenject;

public class InventoryService : IInventoryService
{
    public List<string> FirstPlayerItems { get; set; }
    public List<string> SecondPlayerItems { get; set; }


    private IBlackjackService _blackjackService;
    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService) => _blackjackService = blackjackService;

    public void AddItem(string itemID, PlayerType playerType)
    {
        var item = _blackjackService.GetCardByID(itemID);
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

    public void UseItem(string itemID, NetworkConnection conn)
    {
        var playerType = conn.IsHost ? PlayerType.Player1 : PlayerType.Player2;
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
}
