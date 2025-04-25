using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class TutorialInventoryButtonUI : MonoBehaviour
{
    public Button itemButton;
    public Image itemImage;
    public CardSO item;
    public Shadow shadow;

    private TutorialInventoryUI _tutorialInventoryUI;
    private ITutorialService _tutorialService;

    
    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }
    
    public void Init(TutorialInventoryUI inventoryUI)
    {
        _tutorialInventoryUI = inventoryUI;
        if (item == null)
            return;
        itemButton.onClick.RemoveAllListeners();
        UnityAction action = () =>
        {
            if(inventoryUI != null) inventoryUI.SelectItem(this);
        };
        itemButton.onClick.AddListener(action);
    }

    public void Clear()
    {
        item = null;
        itemButton.onClick.RemoveAllListeners();
        itemImage.sprite = null;
    }

    public void ConfirmClicked()
    {
        _tutorialService.OnExecuteItem(item);
    }
}
