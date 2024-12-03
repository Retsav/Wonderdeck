using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConfigLoader : MonoBehaviour
{
    private static DebugConfigLoader _instance;
    public static DebugConfigLoader Instance => _instance;

    private Dictionary<Type, ScriptableObject> _configs = new();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            LoadConfigs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadConfigs()
    {
        ScriptableObject[] configs = Resources.LoadAll<ScriptableObject>("Configs");
        for (var index = 0; index < configs.Length; index++)
        {
            var config = configs[index];
            Type configType = config.GetType();
            if (!_configs.TryAdd(configType, config))
                Debug.LogWarning($"Duplicate config detected for type: {configType.Name}");
        }
    }
    
    public T GetConfig<T>() where T : ScriptableObject
    {
        Type type = typeof(T);
        if (_configs.TryGetValue(type, out ScriptableObject config))
        {
            return config as T;
        }
        Debug.LogError($"Config of type {type.Name} not found.");
        return null;
    }
}
