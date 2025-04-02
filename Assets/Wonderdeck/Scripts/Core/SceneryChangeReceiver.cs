using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SceneryChangeReceiver : MonoBehaviour
{
    [SerializeField] private string key;
    
    
    
    private IEnvironmentService _environmentService;

    [Inject]
    private void ResolveDependencies(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
        _environmentService.SceneryChangedEvent += OnSceneryChanged;
    }
    

    private void OnSceneryChanged(object sender, SceneryChangedEventArgs e)
    {
        gameObject.SetActive(e.Key == key);
        if(e.Key == key)
            _environmentService.OnLateSceneryChange(e.Key);
    }

    private void OnDestroy()
    {
        _environmentService.SceneryChangedEvent -= OnSceneryChanged;
    }
}
