using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class PostProcessesHandler : MonoBehaviour
{
    [SerializeField] private Volume volume;
    
    
    private IPostProcessingService _processingService;

    [Inject]
    private void ResolveDependencies(IPostProcessingService processingService)
    {
        _processingService = processingService;
    }
    
    private void Start() => _processingService.RegisterVolume(volume);
}
