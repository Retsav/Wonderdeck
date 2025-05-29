using System;
using System.Collections;
using System.Collections.Generic;
using EasyTextEffects;
using FishNet.Object;
using TMPro;
using UnityEngine;
using Zenject;

public class PlayerHealthVisuals : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI firstPlayerHealthLabel;
    [SerializeField] private TextMeshProUGUI secondPlayerHealthLabel;
    [SerializeField] private TextEffect firstTextEffect;
    [SerializeField] private TextEffect secondTextEffect;
    [SerializeField] private GameObject firstPlayerHealthBar;
    [SerializeField] private GameObject secondPlayerHealthBar;

    private IHealthService _healthService;
    private INetworkingService _networkingService;
    private IPostProcessingService _processingService;

    [Inject]
    private void ResolveDependencies(IHealthService healthService, INetworkingService networkingService, IPostProcessingService processingService)
    {
        _healthService = healthService;
        _networkingService = networkingService;
        _processingService = processingService;
    }
    


    public override void OnStartClient()
    {
        _healthService.DamageAppliedEvent += DamageAppliedEvent;
        _processingService.activateTextChange += ActivateTextChange;
        firstPlayerHealthLabel.text = _healthService.FirstPlayerHealth.ToString();
        secondPlayerHealthLabel.text = _healthService.SecondPlayerHealth.ToString();
        if (_networkingService.GetPlayerType(NetworkManager.ClientManager.Connection) != PlayerType.Player2) return;
        firstPlayerHealthBar.transform.Rotate(new Vector3(0f, 180f, 0f));
        secondPlayerHealthBar.transform.Rotate(new Vector3(0f, 180f, 0f));
    }

    private void ActivateTextChange(object sender, EventArgs e)
    {
        firstTextEffect.enabled = true;
        secondTextEffect.enabled = true;
        firstTextEffect.Refresh();
        secondTextEffect.Refresh();
    }

    private void OnDestroy()
    {
        _healthService.DamageAppliedEvent -= DamageAppliedEvent;
        _processingService.activateTextChange -= ActivateTextChange;
    }

    private void DamageAppliedEvent(object sender, DamageAppliedEventArgs e)
    {
        firstPlayerHealthLabel.text = _healthService.FirstPlayerHealth.ToString();
        secondPlayerHealthLabel.text = _healthService.SecondPlayerHealth.ToString();
    }
}
