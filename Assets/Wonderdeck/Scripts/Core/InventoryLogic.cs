using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private IConsequencesService _consequencesService;
    
    private static Random _random = new Random();


    private ItemConfig _itemConfig;

    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService, IConsequencesService consequencesService)
    {
        _inventoryService = inventoryService;
        _consequencesService = consequencesService;
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
        List<CardSO> availableCards = new List<CardSO>();
        if (string.IsNullOrEmpty(e.ItemName))
        {
            if (_itemConfig.itemCards == null || _itemConfig.itemCards.Count == 0)
            {
                Debug.LogError("ItemCards is null or empty.");
                return;
            }
            
            availableCards.AddRange(_itemConfig.itemCards);
            int highestConsequenceTier = _consequencesService.GetHighestConsequenceTierOnPlayer(e.Player);
            if (highestConsequenceTier >= 1 && _itemConfig.consequenceItemCardsFirstTier != null)
                availableCards.AddRange(_itemConfig.consequenceItemCardsFirstTier);
            if (highestConsequenceTier >= 2 && _itemConfig.consequenceItemCardsSecondTier != null)
                availableCards.AddRange(_itemConfig.consequenceItemCardsSecondTier);
            if (highestConsequenceTier >= 3 && _itemConfig.consequenceItemCardsThirdTier != null)
                availableCards.AddRange(_itemConfig.consequenceItemCardsThirdTier);
        }
        else
        {
            if (_itemConfig.itemCards != null && _itemConfig.itemCards.Count > 0)
                availableCards.AddRange(_itemConfig.itemCards);
            if (_itemConfig.consequenceItemCardsFirstTier != null)
                availableCards.AddRange(_itemConfig.consequenceItemCardsFirstTier);
            if (_itemConfig.consequenceItemCardsSecondTier != null)
                availableCards.AddRange(_itemConfig.consequenceItemCardsSecondTier);
            if (_itemConfig.consequenceItemCardsThirdTier != null)
                availableCards.AddRange(_itemConfig.consequenceItemCardsThirdTier);
        }

        List<string> inventoryItems = new List<string>();
        if (string.IsNullOrEmpty(e.ItemName))
        {
            for (int i = 0; i < e.Amount; i++)
            {
                int index = _random.Next(availableCards.Count);
                inventoryItems.Add(availableCards[index].CardId);
            }
        }
        else
        {
            List<CardSO> matchingCards = new List<CardSO>();
            for (int i = 0; i < availableCards.Count; i++)
            {
                if (availableCards[i].name == e.ItemName)
                {
                    matchingCards.Add(availableCards[i]);
                }
            }

            if (matchingCards.Count == 0)
            {
                Debug.LogWarning($"No matching item found for {e.ItemName}");
                return;
            }

            for (int i = 0; i < e.Amount; i++)
            {
                int index = _random.Next(matchingCards.Count);
                inventoryItems.Add(matchingCards[index].CardId);
            }
        }
        
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            string cardId = inventoryItems[i];
            if (e.Player == PlayerType.Player1)
            {
                _inventoryService.AddItem(cardId, PlayerType.Player1);
            }
            else if (e.Player == PlayerType.Player2) AddItemObserverRpc(cardId, PlayerType.Player2);
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

