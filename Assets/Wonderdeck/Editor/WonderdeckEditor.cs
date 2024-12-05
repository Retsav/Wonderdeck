using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class WonderdeckEditor : EditorWindow
{
    private Button _addCardButton;
    private Button _currentlySelectedCardButton;


    private VisualElement _inspectorContent;

    
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
        _inspectorContent = rootVisualElement.Q<VisualElement>("InspectorContent");
        _inspectorContent.Clear();
    }

    private void InitAddCardButton()
    {
        _addCardButton = rootVisualElement.Q<Button>("AddCardButton");
        _addCardButton.clicked += AddCardButtonClicked;
    }

    private void AddCardButtonClicked()
    {
        var newCardSO = (CardSO)CreateInstance(typeof(CardSO));
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Wonderdeck/Assets/Cards/NewCard.asset");
        AssetDatabase.CreateAsset(newCardSO, path);
        newCardSO.cardId = Guid.NewGuid().ToString();
        EditorUtility.SetDirty(newCardSO);
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
            var button = new Button();
            button.text = card.name;
            button.clicked += () => OnCardButtonClicked(card, button);
            scrollView.Add(button);
        }
    }

    private void OnDestroy()
    {
        if (_addCardButton != null)
            _addCardButton.clicked -= AddCardButtonClicked;
    }

    private void OnCardButtonClicked(CardSO card, Button btn)
    {
        _inspectorContent.Clear();
        _currentlySelectedCardButton = btn;
        card.NameChangedEvent -= OnNameChangedEvent;
        var inspectorElement = new InspectorElement(card);
        _inspectorContent.Add(inspectorElement);
        card.NameChangedEvent += OnNameChangedEvent;
    }

    private void OnNameChangedEvent(object sender, string e)
    {
        if (_currentlySelectedCardButton == null)
            return;
        _currentlySelectedCardButton.text = e;
    }
}
