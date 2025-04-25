using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class InventoryTokenUIHandler : MonoBehaviour, ICameraInteractable
{
    [SerializeField] private CanvasGroup uiCanvasGroup;
    [SerializeField] private TextMeshProUGUI uiTextLabel;

    private bool _initialized;
    private bool _hovered;

    private PlayerType _owner;
    [FormerlySerializedAs("_item")] public CardSO item;
    
    private IInventoryService _inventoryService;
    private ISelectModeService _selectModeService;
    private INetworkingService _networkingService;
    private ITutorialService _tutorialService;

    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService, ISelectModeService selectModeService, INetworkingService networkingService, ITutorialService tutorialService)
    {
        _inventoryService = inventoryService;
        _selectModeService = selectModeService;
        _networkingService = networkingService;
        _tutorialService = tutorialService;
    }
    
    private void Start() => uiCanvasGroup.DOFade(0f, 0f);

    private void Update()
    {
        if (!_selectModeService.IsSelectionMode)
            return;
        if (!_hovered)
            return;
        if (!Input.GetKeyDown(KeyCode.E) || !item.ItemIsPersistent) return;
        _selectModeService.OnItemTokenClicked(item, _owner);
    }
    

    public void Init(string itemID, PlayerType owner)
    {
        item = _inventoryService.GetItemByID(itemID);
        if (item == null)
        {
            Debug.LogError($"Can't find inventory item in InventoryTokenUIHandler.");
            return;
        }
        _owner = owner;
        StringBuilder textBuilder = new StringBuilder();
        if (item.ItemIsPersistent) textBuilder.AppendLine("<color=red>PERSISTENT</color>");

        if (!_tutorialService.IsTutorial)
        {
            textBuilder.Append("Owner: ").Append(_owner == PlayerType.Player1
                ? _networkingService.FirstPlayerNickname
                : _networkingService.SecondPlayerNickname);
        }
        else
        {
            textBuilder.Append("Owner: Kizo");
        }

        textBuilder.AppendLine(); 
        textBuilder.Append(item.name);
        uiTextLabel.text = textBuilder.ToString();
        _initialized = true;
    }
    

    public void OnCameraOver()
    {
        if (!_initialized)
            return;
        _hovered = true;
        uiCanvasGroup.DOFade(1f, 0.3f);
    }

    public void OnCameraExit()
    {
        if (!_initialized)
            return;
        _hovered = false;
        uiCanvasGroup.DOFade(0f, 0.3f);
    }
}
