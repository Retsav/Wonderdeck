using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;


[CustomEditor(typeof(WonderdeckButton))]
public class WonderdeckButtonEditor : ButtonEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        SerializedProperty targetImagesProp = serializedObject.FindProperty("targetImages");
        EditorGUILayout.PropertyField(targetImagesProp, true);
        SerializedProperty transitionColorsEnabledProp = serializedObject.FindProperty("transitionColorsEnabled");
        EditorGUILayout.PropertyField(transitionColorsEnabledProp, true);
        SerializedProperty transitionColorsDisabledProp = serializedObject.FindProperty("transitionColorsDisabled");
        EditorGUILayout.PropertyField(transitionColorsDisabledProp, true);
        SerializedProperty buttonTextColorEnabledProp = serializedObject.FindProperty("enabledTextColor");
        EditorGUILayout.PropertyField(buttonTextColorEnabledProp, true);
        SerializedProperty buttonTextColorDisabledProp = serializedObject.FindProperty("disabledTextColor");
        EditorGUILayout.PropertyField(buttonTextColorDisabledProp, true);
        serializedObject.ApplyModifiedProperties();
    }
}
