using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class PlayerConsequenceReceiver : NetworkBehaviour
{
    private PlayerType _playerType;

    private IConsequencesService _consequencesService;
    private ConsequencesConfig _consequencesConfig;
    
    [Inject]
    private void ResolveDependencies(IConsequencesService consequencesService)
    {
        _consequencesService = consequencesService;
    }

    private void Start()
    {
        bool isHost = NetworkManager.ClientManager.Connection.IsHost;
        _consequencesConfig = DebugConfigLoader.Instance.GetConfig<ConsequencesConfig>();
        _playerType = isHost ? PlayerType.Player1 : PlayerType.Player2;
        if (isHost) _consequencesService.ConsequenceAppliedEvent += OnConsequenceApplied;
    }

    private void OnDestroy()
    {
        _consequencesService.ConsequenceAppliedEvent -= OnConsequenceApplied;
    }

    private void OnConsequenceApplied(object sender, ConsequenceAppliedEventArgs e)
    {
        ApplyConsequenceObserverRpc(e.ConsequenceID, e.PlayerType);
    }

    [ObserversRpc]
    private void ApplyConsequenceObserverRpc(string consequenceID, PlayerType playerType)
    {
        if (playerType != _playerType)
            return;
        BaseConsequence consequenceToApply = null;
        for (int i = 0; i < _consequencesConfig.consequenceDatas.Count; i++)
        {
            var consequenceData = _consequencesConfig.consequenceDatas[i];
            if (consequenceData.consequenceID != consequenceID)
                continue;
            consequenceToApply = consequenceData.consequence;
            break;
        }

        if (consequenceToApply == null)
        {
            Debug.LogError($"Consequence to apply with id {consequenceID} not found.");
            return;
        }
        consequenceToApply.Init();
        consequenceToApply.ApplyConsequence();
    }
}
