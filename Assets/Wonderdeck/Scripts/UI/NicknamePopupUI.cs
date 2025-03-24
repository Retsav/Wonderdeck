using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknamePopupUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup nicknamePopupCanvasGroup;

    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button confirmButton;
    

    private void SetCanvasGroupVisibility(CanvasGroup canvasGroup, bool active)
    {
        canvasGroup.alpha = active ? 1f : 0f;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }
    
    private void Start()
    {
        if (PlayerPrefs.HasKey("Nickname"))
        {
            SetCanvasGroupVisibility(mainMenuCanvasGroup, true);
            SetCanvasGroupVisibility(nicknamePopupCanvasGroup, false);
            return;
        }
        SetCanvasGroupVisibility(nicknamePopupCanvasGroup, true);
        SetCanvasGroupVisibility(mainMenuCanvasGroup, false);
        confirmButton.onClick.AddListener(ConfirmNickname);
    }

    private void ConfirmNickname()
    {
        var nickname = nicknameInputField.text;
        PlayerPrefs.SetString("Nickname", nickname);
        SetCanvasGroupVisibility(mainMenuCanvasGroup, true);
        SetCanvasGroupVisibility(nicknamePopupCanvasGroup, false);
    }


    private void OnDestroy()
    {
        confirmButton.onClick.RemoveAllListeners();
    }
}
