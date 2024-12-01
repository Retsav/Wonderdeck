using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableType))]
public class SerializableTypeDrawer : PropertyDrawer
{
    private TypeFilterAttribute _filterAttribute;
    private string[] typeNames, typeFullNames;

    private void Initialize()
    {
        if (typeFullNames != null) return;
        _filterAttribute = (TypeFilterAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(TypeFilterAttribute));
        var filteredTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => _filterAttribute == null ? DefaultFiler(t) : _filterAttribute.Filter(t))
            .ToArray();
        typeNames = filteredTypes.Select(t => t.ReflectedType == null ? t.Name : $"t.ReflectedTypeName + t.Name")
            .ToArray();
        typeFullNames = filteredTypes.Select(t => t.AssemblyQualifiedName).ToArray();
    }

    private string GetFriendlyTypeName(string assemblyQualifiedName)
    {
        if (string.IsNullOrEmpty(assemblyQualifiedName))
            return "Effect";
        var type = Type.GetType(assemblyQualifiedName);
        return type != null ? type.Name : "Unknown Type";
    }

    private static bool DefaultFiler(Type type) => !type.IsAbstract && !type.IsInterface && !type.IsGenericType;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        Initialize();
        var typeIdProperty = property.FindPropertyRelative("assemblyQualifiedName");
        if (string.IsNullOrEmpty(typeIdProperty.stringValue))
        {
            typeIdProperty.stringValue = typeFullNames.First();
            property.serializedObject.ApplyModifiedProperties();
        }

        string currentTypeName = GetFriendlyTypeName(typeIdProperty.stringValue);
        label.text = "";
        var currentIndex = Array.IndexOf(typeFullNames, typeIdProperty.stringValue);
        var selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);
        if (selectedIndex >= 0 && selectedIndex != currentIndex)
        {
            typeIdProperty.stringValue = typeFullNames[selectedIndex];
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
