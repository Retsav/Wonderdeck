using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class InventoryTokensVisual : NetworkBehaviour
{
    [SerializeField] private GameObject itemTokenPrefab;
    [SerializeField] private Transform firstItemTokenSpawnPoint;
    [SerializeField] private float itemTokensSpacing;

    private IBlackjackService _blackjackService;
    private IInventoryService _inventoryService;

    private readonly List<GameObject> _spawnedTokens = new();

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, IInventoryService inventoryService)
    {
        _blackjackService = blackjackService;
        _inventoryService = inventoryService;
    }
    
    
    public override void OnStartClient()
    {
        _blackjackService.RoundEnd += OnRoundEnd;
        if (!NetworkManager.ClientManager.Connection.IsHost) return;
        _blackjackService.CardPlayed += OnCardPlayed;
    }

    private void OnRoundEnd(object sender, EventArgs e)
    {
        for (int i = _spawnedTokens.Count - 1; i >= 0; i--)
        {
            Destroy(_spawnedTokens[i].gameObject);
        }
        _spawnedTokens.Clear();
    }

    private void OnCardPlayed(object sender, CardPlayedEventArgs e)
    {
        var card = _blackjackService.GetCardByID(e.CardID);
        if (card != null)
            return;
        var item = _inventoryService.GetItemByID(e.CardID);
        if (item == null)
        {
            Debug.LogError($"Item {item.name} is null in InventoryService.");
            return;
        }
        SpawnItemTokenObserverRpc(item.CardId);
    }

    [ObserversRpc]
    private void SpawnItemTokenObserverRpc(string cardId)
    {
        var go = Instantiate(
            itemTokenPrefab, 
            firstItemTokenSpawnPoint.position + new Vector3(_spawnedTokens.Count * itemTokensSpacing, 0f, 0f),
            Quaternion.identity
            );

        go.transform.eulerAngles = new Vector3(0f, NetworkManager.ClientManager.Connection.IsHost ? 0f : 180f, 0f);
        _spawnedTokens.Add(go);
        if (go.TryGetComponent(out InventoryTokenUIHandler uiHandler)) uiHandler.Init(cardId);
        else
            Debug.LogError("Couldnt find InventoryTokenUIHandler in Spawned Token.");
    }

    private void OnDestroy()
    {
        _spawnedTokens.Clear();
        if (_blackjackService == null)
            return;
        _blackjackService.CardPlayed -= OnCardPlayed;
        _blackjackService.RoundEnd -= OnRoundEnd;
    }
}
