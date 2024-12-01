using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomEditor(typeof(CardSO))]
public class CardSOEditor : Editor
{
    private TemplateContainer _root;
    private TextField _textField;
    
    private VisualElement _cardFaceSprite;
    private VisualElement _cardBackSprite;

    private ObjectField _cardFaceObjectField;
    private ObjectField _cardBackObjectField;
    
    private CardSO _cardTarget;
    
    private VisualElement _cardDrawListEffectsContainer;
    private VisualElement _cardPlayListEffectsContainer;
    private VisualElement _cardDiscardListEffectsContainer;
    private DropdownField _cardDrawDropdownField;
    private DropdownField _cardPlayDropdownField;
    private DropdownField _cardDiscardDropdownField;
    private Type[] _effectTypes;
    public override VisualElement CreateInspectorGUI()
    {
        _cardTarget = _cardTarget ? _cardTarget : (CardSO)target;
        if (target == null && _cardTarget != null)
        {
            target = _cardTarget;
        }
        _effectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => typeof(ICardEffect).IsAssignableFrom(t) && !t.IsAbstract && t.IsSubclassOf(typeof(ScriptableObject)))
            .ToArray();    
        var visualTree =
              AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Wonderdeck/Assets/UI Toolkit/CardInspector.uxml");
        _root = visualTree.CloneTree();
        var styleSheet =
              AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Wonderdeck/Assets/UI Toolkit/CardInspector.uss");
        _root.styleSheets.Add(styleSheet);
        _root.Bind(serializedObject);
        InitializeLogic();
        return _root; 
    }

    private void InitializeLogic()
    {
        InitTextField();
        InitSpriteContainer();
        InitEffectsLists();
    }

    private void InitEffectsLists()
    {
        _cardDrawListEffectsContainer = _root.Q<VisualElement>("DrawEffectsContainer");
        _cardPlayListEffectsContainer = _root.Q<VisualElement>("PlayEffectsContainer");
        _cardDiscardListEffectsContainer = _root.Q<VisualElement>("DiscardEffectsContainer");
        CreateListSection(_cardDrawListEffectsContainer, _cardTarget.DrawCardEffects, "DrawCardEffects");
        CreateListSection(_cardPlayListEffectsContainer, _cardTarget.PlayCardEffects, "PlayCardEffects");
        CreateListSection(_cardDiscardListEffectsContainer, _cardTarget.DiscardCardEffects, "DiscardCardEffects");
    }

    private void CreateListSection(VisualElement parent, List<ScriptableObject> effects, string propertyName)
    {
        RefreshList(parent, propertyName);
        parent.Q<Button>("AddButton").clicked += () =>
        {
            var arrayProperty = serializedObject.FindProperty(propertyName);
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            var newElement = arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
            newElement.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
            RefreshList(parent, propertyName);
        };
        parent.Q<Button>("RemoveButton").clicked += () =>
        {
            var arrayProperty = serializedObject.FindProperty(propertyName);
            if (effects.Count <= 0)
                return;
            arrayProperty.DeleteArrayElementAtIndex(arrayProperty.arraySize - 1);
            serializedObject.ApplyModifiedProperties();
            RefreshList(parent, propertyName);
        };
    }

    private void RefreshList(VisualElement parent, string propertyName)
    {
        parent.Q<ScrollView>("ScrollView").Clear();
        var arrayProperty = serializedObject.FindProperty(propertyName);
        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            var elementProperty = arrayProperty.GetArrayElementAtIndex(i);
            var objectField = new ObjectField()
            {
                objectType = typeof(ScriptableObject),
                value = elementProperty.objectReferenceValue
            };
            objectField.RegisterValueChangedCallback(evt =>
            {
                elementProperty.objectReferenceValue = evt.newValue as ScriptableObject;
                serializedObject.ApplyModifiedProperties();
            });
            parent.Q<ScrollView>("ScrollView").Add(objectField);
        }
    }

    private void CreateListItem(VisualElement parent, List<ScriptableObject> effects, int index)
    {
        var itemContainer = new VisualElement();
        parent.Add(itemContainer);
        var objectField = new ObjectField()
        {
            objectType = typeof(ScriptableObject),
            value = effects[index],
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            effects[index] = (ScriptableObject)evt.newValue; // Update the list with the new value
            EditorUtility.SetDirty(_cardTarget);
        });
        itemContainer.Add(objectField);
        var removeButton = new Button(() =>
        {
            effects.RemoveAt(index);
            EditorUtility.SetDirty(_cardTarget);
            RefreshList(parent.parent, effects);
        })
        {
            text = "X"
        };
        itemContainer.Add(removeButton);
    }
    
    private void RefreshList(VisualElement parent, List<ScriptableObject> effects)
    {
        parent.Clear();
        for (int i = 0; i < effects.Count; i++)
        {
            CreateListItem(parent, effects, i);
        }
    }
    

    private void InitSpriteContainer()
    {
        _cardBackSprite = _root.Q<VisualElement>("CardBackSprite");
        _cardFaceSprite = _root.Q<VisualElement>("CardFaceSprite");
        _cardBackObjectField = _root.Q<ObjectField>("CardBackObjectField");
        _cardFaceObjectField = _root.Q<ObjectField>("CardFaceObjectField");
        _cardBackObjectField.objectType = typeof(Sprite);
        _cardFaceObjectField.objectType = typeof(Sprite);
        _cardBackObjectField.RegisterValueChangedCallback(UpdateCardSprites);
        _cardFaceObjectField.RegisterValueChangedCallback(UpdateCardSprites);
        _cardFaceObjectField.bindingPath = "CardFace";
        _cardBackObjectField.bindingPath = "CardBack";
        _cardBackSprite.style.backgroundImage = new StyleBackground(_cardTarget.CardBack.texture);
        _cardFaceSprite.style.backgroundImage = new StyleBackground(_cardTarget.CardFace.texture);
    }

    private void UpdateCardSprites(ChangeEvent<Object> evt)
    {
        _cardBackSprite.style.backgroundImage = _cardTarget.CardBack != null ? new StyleBackground(_cardTarget.CardBack.texture) : null;
        _cardFaceSprite.style.backgroundImage = _cardTarget.CardFace != null ? new StyleBackground(_cardTarget.CardFace.texture) : null;
    }

    private void InitTextField()
    {
        _textField = _root.Q<TextField>("CardName");
        _textField.value = _cardTarget.name;
        _textField.RegisterCallback<FocusOutEvent>(ModifyName);
    }

    private void ModifyName(FocusOutEvent evt)
    {
        Undo.RecordObject(_cardTarget, "Modify Card Name");
        string assetPath = AssetDatabase.GetAssetPath(_cardTarget);
        AssetDatabase.RenameAsset(assetPath, _textField.value);
        _cardTarget.name = _textField.value;
        EditorUtility.SetDirty(_cardTarget);
        _cardTarget.OnNameChanged(_textField.value);
    }

    private void OnDestroy()
    {
        if(_textField != null) _textField.UnregisterCallback<FocusOutEvent>(ModifyName);
        if(_cardBackObjectField != null) _cardBackObjectField.UnregisterValueChangedCallback(UpdateCardSprites);
        if (_cardFaceObjectField != null) _cardFaceObjectField.UnregisterValueChangedCallback(UpdateCardSprites);
    }
}
