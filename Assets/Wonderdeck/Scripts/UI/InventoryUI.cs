using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class InventoryUI : NetworkBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private GameObject inventoryButtonsParent;
    [SerializeField] private TextMeshProUGUI itemDescriptionLabel;
    [SerializeField] private TextMeshProUGUI itemNameLabel;
    [SerializeField] private Button confirmButton;
    


    private bool _inventoryPopupOpened;
    
    private InventoryButtonUI _currentlySelectedInventoryButton;
    
    private IInventoryService _inventoryService;
    private IBlackjackService _blackjackService;
    private IAudioService _audioService;
    private ISelectModeService _selectModeService;

    private AudioConfig _audioConfig;

    private Sequence _popupSequence;

    
    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService, IBlackjackService blackjackService, IAudioService audioService, ISelectModeService selectModeService)
    {
        _inventoryService = inventoryService;
        _blackjackService = blackjackService;
        _audioService = audioService;
        _selectModeService = selectModeService;
    }

    private void Start()
    {
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();
        HideGroup();
        ClearItemButtons();
        _blackjackService.GameStateSet += OnGameStateSet;
        itemDescriptionLabel.text = "";
        itemNameLabel.text = "";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
    }

    private bool CanUseItems(BlackjackState state)
    {
        return state switch
        {
            BlackjackState.Intermission => false,
            BlackjackState.Player1Turn => NetworkManager.ClientManager.Connection.IsHost,
            BlackjackState.Player2Turn => !NetworkManager.ClientManager.Connection.IsHost,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
    private void OnGameStateSet(object sender, GameStateSetEventArgs e) => confirmButton.interactable = CanUseItems(e.State);

    private void OnConfirmClicked()
    {
        if (_currentlySelectedInventoryButton == null)
            return;
        _currentlySelectedInventoryButton.ConfirmClicked();
        _currentlySelectedInventoryButton.shadow.enabled = false;
        _currentlySelectedInventoryButton = null;
        itemDescriptionLabel.text = "";
        itemNameLabel.text = "";
        Close();
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
        if (_selectModeService.IsSelectionMode)
            return;
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
        if (_popupSequence != null)
        {
            if (_popupSequence.IsActive()) _popupSequence.Kill();
            _popupSequence = null;
        }
        _popupSequence = DOTween.Sequence();    
        _popupSequence.Join(rectTransform.DOAnchorPosX(0f, 0.3f));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        confirmButton.interactable = CanUseItems(_blackjackService.BlackjackState);
        ShowGroup();
        PopulateItemButtons();
    }

    public void Close()
    {
        if (_popupSequence != null)
        {
            if (_popupSequence.IsActive()) _popupSequence.Kill();
            _popupSequence = null;
        }
        _popupSequence = DOTween.Sequence();    
        _popupSequence.Join(rectTransform.DOAnchorPosX(600f, 0.3f));
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        HideGroup();
        ClearItemButtons();
    }

    public void SelectItem(InventoryButtonUI button)
    {
        if (_currentlySelectedInventoryButton != null) 
            _currentlySelectedInventoryButton.shadow.enabled = false;
        _currentlySelectedInventoryButton = button;
        _currentlySelectedInventoryButton.shadow.enabled = true;
        if (!string.IsNullOrEmpty(_currentlySelectedInventoryButton.itemID))
        {
            var item = _inventoryService.GetItemByID(_currentlySelectedInventoryButton.itemID);
            if (item == null)
            {
                Debug.LogError($"Item with ID {_currentlySelectedInventoryButton.itemID} not found");
                return;
            }
            itemNameLabel.text = item.name;
            itemDescriptionLabel.text = item.description;
        } 
    }

    private void ClearItemButtons()
    {
        foreach (Transform child in inventoryButtonsParent.transform)
        {
            if (child.TryGetComponent(out InventoryButtonUI inventoryButton))
                inventoryButton.Clear();
            else
                Debug.LogError("Child of InventoryButtonParent doesnt have InventoryButtonUI attached.");
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
