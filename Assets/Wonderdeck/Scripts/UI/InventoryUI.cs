using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class InventoryUI : NetworkBehaviour
{
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private GameObject inventoryButtonsParent;


    private bool _inventoryPopupOpened;
    
    private IInventoryService _inventoryService;
    private IBlackjackService _blackjackService;
    private IAudioService _audioService;

    private AudioConfig _audioConfig;


    
    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService, IBlackjackService blackjackService, IAudioService audioService)
    {
        _inventoryService = inventoryService;
        _blackjackService = blackjackService;
        _audioService = audioService;
    }

    private void Start()
    {
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();
        HideGroup();
        ClearItemButtons();
    }

    public override void OnStartClient()
    {
        _inventoryService.InventoryRefreshed += OnRefreshInventory;
    }


    private void OnDestroy()
    {
        if(_inventoryService != null) _inventoryService.InventoryRefreshed -= OnRefreshInventory;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            switch (_inventoryPopupOpened)
            {
                case true:
                    Close();
                    break;
                default:
                    Open();
                    break;
            }

    }

    public void Open()
    {
        _audioService.OnPlaySoundLocal(Vector3.zero, _audioConfig.openInventoryPath);
        ShowGroup();
        PopulateItemButtons();
    }

    public void Close()
    {
        HideGroup();
        ClearItemButtons();
    }

    private void ClearItemButtons()
    {
        foreach (Transform child in inventoryButtonsParent.transform)
        {
            if (child.TryGetComponent(out InventoryButtonUI inventoryButton))
                inventoryButton.Clear();
            else
            {
                Debug.LogError("Child of InventoryButtonParent doesnt have InventoryButtonUI attached.");
                continue;
            }
        }
    }

    private void PopulateItemButtons()
    {
        List<string> itemsID = ClientManager.Connection.IsHost
            ? _inventoryService.FirstPlayerItems
            : _inventoryService.SecondPlayerItems;
        if (itemsID.Count == 0) return;
        var i = 0;
        foreach (Transform child in inventoryButtonsParent.transform)
        {
            if (i >= itemsID.Count) break;
            if (child.TryGetComponent(out InventoryButtonUI inventoryButton))
            {
                if (string.IsNullOrEmpty(inventoryButton.itemID))
                {
                    inventoryButton.itemID = itemsID[i];
                    inventoryButton.itemImage.sprite = _blackjackService.GetCardFaceSprite(inventoryButton.itemID);
                    inventoryButton.Init(this);
                    i++;
                }
            }
            else
            {
                Debug.LogError("Child of InventoryButtonParent doesnt have InventoryButtonUI attached.");
                continue;
            }
        }
    }
    

    private void OnRefreshInventory(object sender, EventArgs e)
    {
        if (!_inventoryPopupOpened) return;
        ClearItemButtons();
        PopulateItemButtons();
    }

    private void HideGroup()
    {
        inventoryCanvasGroup.blocksRaycasts = false;
        inventoryCanvasGroup.interactable = false;
        inventoryCanvasGroup.alpha = 0f;
        _inventoryPopupOpened = false;
    }
    
    private void ShowGroup()
    {
        inventoryCanvasGroup.blocksRaycasts = true;
        inventoryCanvasGroup.interactable = true;
        inventoryCanvasGroup.alpha = 1f;
        _inventoryPopupOpened = true;
    }
}
