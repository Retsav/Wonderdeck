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
    private ISelectModeService _selectModeService;
    private IAudioService _audioService;

    private AudioConfig _audioConfig;

    private readonly List<GameObject> _spawnedTokens = new();

    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, IInventoryService inventoryService, ISelectModeService selectModeService, IAudioService audioService)
    {
        _blackjackService = blackjackService;
        _inventoryService = inventoryService;
        _selectModeService = selectModeService;
        _audioService = audioService;
    }
    
    
    public override void OnStartClient()
    {
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();
        _blackjackService.RoundEnd += OnRoundEnd;
        _selectModeService.RequestSelectionEffectExecution += OnRequestSelection;
        if (!NetworkManager.ClientManager.Connection.IsHost) return;
        _blackjackService.CardPlayed += OnCardPlayed;
        
    }

    private void OnRequestSelection(object sender, SelectionEffectExecutionEventArgs e)
    {
        RequestTokenDeletionServerRpc(e.SelectedCardID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestTokenDeletionServerRpc(string selectedCardID)
    {
        DeleteTokenObserverRpc(selectedCardID);
    }

    [ObserversRpc]
    private void DeleteTokenObserverRpc(string selectedCardID)
    {
        GameObject goForDeletion = null;
        for (int i = 0; i < _spawnedTokens.Count; i++)
        {
            if (!_spawnedTokens[i].TryGetComponent(out InventoryTokenUIHandler inventoryToken)) continue;
            if (inventoryToken.item.CardId != selectedCardID)
                continue;
            goForDeletion = _spawnedTokens[i];
        }

        if (goForDeletion != null)
        {
            _spawnedTokens.Remove(goForDeletion);
            Destroy(goForDeletion);
        }

    }

    private void OnRoundEnd(object sender, EventArgs e)
    {
        for (int i = _spawnedTokens.Count - 1; i >= 0; i--) Destroy(_spawnedTokens[i].gameObject);
        _spawnedTokens.Clear();
    }

    private void OnCardPlayed(object sender, CardPlayedEventArgs e)
    {
        if (e.PlayType != PlayType.Play)
            return;
        var card = _blackjackService.GetCardByID(e.CardID);
        if (card != null)
            return;
        var item = _inventoryService.GetItemByID(e.CardID);
        
        if (item == null)
        {
            Debug.LogError($"Item {item.name} is null in InventoryService.");
            return;
        }
        SpawnItemTokenObserverRpc(item.CardId, e.PlayerType);
    }

    [ObserversRpc]
    private void SpawnItemTokenObserverRpc(string cardId, PlayerType owner)
    {
        var go = Instantiate(
            itemTokenPrefab, 
            firstItemTokenSpawnPoint.position + new Vector3(_spawnedTokens.Count * itemTokensSpacing, 0f, 0f),
            Quaternion.identity
            );
        go.transform.eulerAngles = new Vector3(0f, NetworkManager.ClientManager.Connection.IsHost ? 0f : 180f, 0f);
        _spawnedTokens.Add(go);
        if (go.TryGetComponent(out InventoryTokenUIHandler uiHandler)) uiHandler.Init(cardId, owner);
        else
            Debug.LogError("Couldnt find InventoryTokenUIHandler in Spawned Token.");
        _audioService.OnPlaySoundAtPosition(go.transform.position, _audioConfig.tokenPlaced);
    }

    private void OnDestroy()
    {
        _spawnedTokens.Clear();
        _selectModeService.RequestSelectionEffectExecution -= OnRequestSelection;
        if (_blackjackService == null)
            return;
        _blackjackService.CardPlayed -= OnCardPlayed;
        _blackjackService.RoundEnd -= OnRoundEnd;
    }
}
