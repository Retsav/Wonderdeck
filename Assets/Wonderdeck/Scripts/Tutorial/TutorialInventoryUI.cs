using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class TutorialInventoryUI : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    [SerializeField] private GameObject inventoryButtonsParent;
    [SerializeField] private TextMeshProUGUI itemDescriptionLabel;
    [SerializeField] private TextMeshProUGUI itemNameLabel;
    [SerializeField] private Button confirmButton;
    
    

    private bool _inventoryPopupOpened;
    public bool InventoryPopupUnlocked = false;
    
    private TutorialInventoryButtonUI _currentlySelectedInventoryButton;
    

    private AudioConfig _audioConfig;

    private Sequence _popupSequence;

    public ITutorialService _tutorialService;

    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    private void Start()
    {
        HideGroup();
        ClearItemButtons();
        itemDescriptionLabel.text = "";
        itemNameLabel.text = "";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);

        _tutorialService.DialogueFinished += OnDialogueFinished;
    }

    private void OnDestroy()
    {
        _tutorialService.DialogueFinished -= OnDialogueFinished;
    }

    private void OnDialogueFinished(object sender, int e)
    {
        switch (_tutorialService.TutorialStep)
        {
            case 9:
                InventoryPopupUnlocked = true;
                break;
            case 16:
                InventoryPopupUnlocked = true;
                break;
            case 21:
                InventoryPopupUnlocked = true;
                break;
        }
    }

    private void Update()
    {
        if (!InventoryPopupUnlocked)
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
    
    public void Open()
    {
        if (_popupSequence != null)
        {
            if (_popupSequence.IsActive()) _popupSequence.Kill();
            _popupSequence = null;
        }
        _popupSequence = DOTween.Sequence();    
        _popupSequence.Join(rectTransform.DOAnchorPosX(0f, 0.3f));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        confirmButton.interactable = true;
        ShowGroup();
        PopulateItemButtons();
    }

    private void PopulateItemButtons()
    {
        var i = 0;
        foreach (Transform child in inventoryButtonsParent.transform)
        {
            if (i >= _tutorialService.FirstPlayerItems.Count) break;
            if (child.TryGetComponent(out TutorialInventoryButtonUI inventoryButton))
            {
                if (inventoryButton.item == null)
                {
                    inventoryButton.item = _tutorialService.FirstPlayerItems[i];
                    inventoryButton.itemImage.sprite = inventoryButton.item.CardFace;
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
        InventoryPopupUnlocked = false;
    }

    private void ClearItemButtons()
    {
        foreach (Transform child in inventoryButtonsParent.transform)
        {
            if (child.TryGetComponent(out TutorialInventoryButtonUI inventoryButton))
                inventoryButton.Clear();
            else
                Debug.LogError("Child of InventoryButtonParent doesnt have InventoryButtonUI attached.");
        }
    }

    private void RefreshInventory()
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

    public void SelectItem(TutorialInventoryButtonUI button)
    {
        if (_currentlySelectedInventoryButton != null) 
            _currentlySelectedInventoryButton.shadow.enabled = false;
        _currentlySelectedInventoryButton = button;
        _currentlySelectedInventoryButton.shadow.enabled = true;
        if (_currentlySelectedInventoryButton.item == null) return;
        var item = _currentlySelectedInventoryButton.item;
        if (item == null)
        {
            Debug.LogError($"Item with ID {_currentlySelectedInventoryButton.item} not found");
            return;
        }
        itemNameLabel.text = item.name;
        itemDescriptionLabel.text = item.description;
    }
}
