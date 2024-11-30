using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class WonderdeckEditor : EditorWindow
{
    private Button addCardButton;
    
    
    [MenuItem("Wonderdeck/Wonderdeck Editor")]
    public static void ShowEditor()
    {
        var window = GetWindow<WonderdeckEditor>();
        window.titleContent = new GUIContent("Wonderdeck Editor");
        window.minSize = new Vector2(400, 300);
    }

    public void CreateGUI()
    {
        var visualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Wonderdeck/Assets/UI Toolkit/CardEditor.uxml");
        visualTree.CloneTree(rootVisualElement);
        InitScrollView();
        InitAddCardButton();
    }

    private void InitAddCardButton()
    {
        addCardButton = rootVisualElement.Q<Button>("AddCardButton");
        addCardButton.clicked += AddCardButtonClicked;
    }

    private void AddCardButtonClicked()
    {
        var newCardSO = (CardSO)CreateInstance(typeof(CardSO));
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Wonderdeck/Assets/Cards/NewCard.asset");
        AssetDatabase.CreateAsset(newCardSO, path);
        Debug.Log("Added Card!");
        InitScrollView();
    }

    private void InitScrollView()
    {
        var scrollView = rootVisualElement.Q<ScrollView>("CardScrollView");
        scrollView.Clear();
        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets/Wonderdeck/Assets/Cards" });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            CardSO card = AssetDatabase.LoadAssetAtPath<CardSO>(path);
            if (card == null)
                return;
            var button = new Button(() => OnCardButtonClicked(card))
            {
                text = card.name
            };
            scrollView.Add(button);
        }
    }

    private void OnDestroy()
    {
        if (addCardButton != null)
            addCardButton.clicked -= AddCardButtonClicked;
    }

    private void OnCardButtonClicked(CardSO card)
    {
        Debug.Log("Debug hello!");
    }
}
