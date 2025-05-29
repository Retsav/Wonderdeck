using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using EasyTextEffects;
using TMPro;
using Zenject;

public class LightHallucination : BaseConsequence
{
    private INetworkingService _networkingService;
    private IPostProcessingService _processingService;
    
    
    
    
    private bool _consequenceActive = false;


    [Inject]
    private void ResolveDependencies(INetworkingService networkingService, IPostProcessingService postProcessingService)
    {
        _networkingService = networkingService;
        _processingService = postProcessingService;
    }
    
    public override void ApplyConsequence()
    {
        Debug.Log("Consequence Applied.");
        _processingService.OnActivateTextChange();
    }
    
    
    public override void RemoveConsequence()
    {
        Debug.Log("Consequence removed");

        _consequenceActive = false;
    }
}
