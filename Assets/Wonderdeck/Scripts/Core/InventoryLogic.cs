using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using UnityEngine;
using Zenject;
using Random = System.Random;

public class InventoryLogic : NetworkBehaviour
{
    [SerializeField] private GameObject dummyTokenPrefab;
    [SerializeField] private Transform player1DummyTokenSpawnPoint;
    [SerializeField] private Transform player1DummyTokenTargetPoint;
    [SerializeField] private Transform player2DummyTokenSpawnPoint;
    [SerializeField] private Transform player2DummyTokenTargetPoint;
    
    private Queue<SpawnRequest> spawnQueue = new Queue<SpawnRequest>();
    private bool isProcessingQueue = false;

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
            if (e.Player == PlayerType.Player2) 
                AddItemObserverRpc(inventoryItems[i], PlayerType.Player2);
        }

        SpawnItemsVisualObserverRpc(e.Player, e.Amount);
    }
    
    
    [ObserversRpc]
    private void SpawnItemsVisualObserverRpc(PlayerType player, int amount)
    {
        spawnQueue.Enqueue(new SpawnRequest(player, amount));
        
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessSpawnQueue());
        }
    }
    
    private IEnumerator ProcessSpawnQueue()
    {
        isProcessingQueue = true;
        
        while (spawnQueue.Count > 0)
        {
            SpawnRequest request = spawnQueue.Dequeue();
            for (int i = 0; i < request.amount; i++)
            {
                GameObject tokenGO;
                Tween tween;
                
                if (request.player == PlayerType.Player1)
                {
                    tokenGO = Instantiate(dummyTokenPrefab, player1DummyTokenSpawnPoint.position, Quaternion.identity);
                    tween = tokenGO.transform.DOMoveX(player1DummyTokenTargetPoint.position.x, 0.6f);
                }
                else 
                {
                    tokenGO = Instantiate(dummyTokenPrefab, player2DummyTokenSpawnPoint.position, Quaternion.identity);
                    tween = tokenGO.transform.DOMoveX(player2DummyTokenTargetPoint.position.x, 0.6f);
                }
                
                yield return tween.WaitForCompletion();
                Destroy(tokenGO);
                yield return new WaitForSeconds(0.2f);
            }
        }

        isProcessingQueue = false;
    }


    [ObserversRpc]
    private void AddItemObserverRpc(string id, PlayerType player)
    {
        _inventoryService.AddItem(id, player);
    }


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

    private void OnDestroy() => Unsubscribe();
    public override void OnStopClient() => Unsubscribe();

    private void Unsubscribe()
    {
        _inventoryService.RequestInventoryUsage -= OnInventoryUsageRequested;
        _inventoryService.RequestInventory -= OnInventoryRequested;
        _inventoryService.ItemsDealRequested -= DealInventoryItems;
        _inventoryService.FirstPlayerItems = null;
        _inventoryService.SecondPlayerItems = null;
    }
    
    private struct SpawnRequest
    {
        public PlayerType player;
        public int amount;

        public SpawnRequest(PlayerType player, int amount)
        {
            this.player = player;
            this.amount = amount;
        }
    }
}

