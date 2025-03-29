using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BlackScreenHandlerUI : MonoBehaviour
{
    [SerializeField] private Image blackScreenImage;


    private IEnvironmentService _environmentService;

    
    [Inject]
    private void ResolveDependencies(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
    }
    
    private void Start()
    {
        blackScreenImage.enabled = false;
        _environmentService.EarlySceneryChangedEvent += OnEarlySceneryChangedEvent;
        _environmentService.LateSceneryChangedEvent += OnLateSceneryChangedEvent;
    }

    private void OnLateSceneryChangedEvent(object sender, SceneryChangedEventArgs e)
    {
        StartCoroutine(LateSceneryChangeDelay());
        //blackScreenImage.enabled = false;
    }

    private IEnumerator LateSceneryChangeDelay()
    {
        yield return new WaitForSeconds(0.15f);
        blackScreenImage.enabled = false;
    }

    private void OnEarlySceneryChangedEvent(object sender, SceneryChangedEventArgs e) => blackScreenImage.enabled = true;

    private void OnDestroy()
    {
        _environmentService.EarlySceneryChangedEvent -= OnEarlySceneryChangedEvent;
        _environmentService.LateSceneryChangedEvent -= OnLateSceneryChangedEvent;
    }
}
