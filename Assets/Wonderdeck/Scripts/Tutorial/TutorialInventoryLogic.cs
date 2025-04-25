using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class TutorialInventoryLogic : MonoBehaviour
{
    [SerializeField] private GameObject dummyTokenPrefab;
    [SerializeField] private Transform player1DummyTokenSpawnPoint;
    [SerializeField] private Transform player1DummyTokenTargetPoint;
    [SerializeField] private Transform player2DummyTokenSpawnPoint;
    [SerializeField] private Transform player2DummyTokenTargetPoint;
    [SerializeField] private TutorialDamageCalculatorUI tutorialDamageCalculatorUI;
    


    [SerializeField] private GameObject itemTokenPrefab;
    [SerializeField] private Transform firstItemTokenSpawnPoint;
    [SerializeField] private float itemTokensSpacing;
    
    private IInventoryService _inventoryService;
    private ITutorialService _tutorialService;
    
    private Queue<SpawnRequest> spawnQueue = new Queue<SpawnRequest>();
    private bool isProcessingQueue;
    
    private readonly List<GameObject> _spawnedTokens = new();


    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService, ITutorialService tutorialService)
    {
        _inventoryService = inventoryService;
        _tutorialService = tutorialService;
    }
    
    public void DealItem(PlayerType playerType, string itemID)
    {
        var item = _inventoryService.GetItemByID(itemID);
        var targetList = playerType == PlayerType.Player1
            ? _tutorialService.FirstPlayerItems
            : _tutorialService.SecondPlayerItems;
        targetList.Add(item);
        SpawnItemVisual(playerType);
    }

    public void ArbiterPlayItem(CardSO cardSO)
    {
        SpawnToken(cardSO);
    }

    private void SpawnToken(CardSO cardSO)
    {
        var go = Instantiate(
            itemTokenPrefab, 
            firstItemTokenSpawnPoint.position + new Vector3(_spawnedTokens.Count * itemTokensSpacing, 0f, 0f),
            Quaternion.identity
        );
        go.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        _spawnedTokens.Add(go);
        if (go.TryGetComponent(out InventoryTokenUIHandler uiHandler)) uiHandler.Init(cardSO.CardId, PlayerType.Player2);
        else
            Debug.LogError("Couldnt find InventoryTokenUIHandler in Spawned Token.");
        tutorialDamageCalculatorUI.ChangeModifier(-50);
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }
    
    public void DestroyPersistentItems()
    {
        Destroy(_spawnedTokens[0].gameObject);
        _spawnedTokens.Clear();
        tutorialDamageCalculatorUI.ChangeModifier(0);
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }

    private void SpawnItemVisual(PlayerType player)
    {
        spawnQueue.Enqueue(new SpawnRequest(player, 1));
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
