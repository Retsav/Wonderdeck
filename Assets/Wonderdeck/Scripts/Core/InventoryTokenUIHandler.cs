using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

public class InventoryTokenUIHandler : MonoBehaviour, ICameraInteractable
{
    [SerializeField] private CanvasGroup uiCanvasGroup;
    [SerializeField] private TextMeshProUGUI uiTextLabel;

    private bool _initialized;
    
    private CardSO _item;
    private IInventoryService _inventoryService;

    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }
    
    private void Start() => uiCanvasGroup.DOFade(0f, 0f);

    public void Init(string itemID)
    {
        _item = _inventoryService.GetItemByID(itemID);
        if (_item == null)
        {
            Debug.LogError($"Can't find inventory item in InventoryTokenUIHandler.");
            return;
        }
        uiTextLabel.text = $"USED ITEM:\n{_item.name}";
        _initialized = true;
    }
    public void OnCameraOver()
    {
        if (!_initialized)
            return;
        uiCanvasGroup.DOFade(1f, 0.3f);
    }

    public void OnCameraExit()
    {
        if (!_initialized)
            return;
        uiCanvasGroup.DOFade(0f, 0.3f);
    }
}
