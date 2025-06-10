using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class VoiceActorManager : NetworkBehaviour
{
    private List<string> hitVoiceLines;
    private List<string> standVoiceLines;


    private AudioConfig _audioConfig;
    private IAudioService _audioService;
    private IBlackjackService _blackjackService;
    


    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService, IAudioService audioService)
    {
        _blackjackService = blackjackService;
        _audioService = audioService;
    }

    public override void OnStartClient()
    {
        _audioConfig = DebugConfigLoader.Instance.GetConfig<AudioConfig>();

        
        hitVoiceLines = _audioConfig.lilyVoiceLinesHitPaths;
        
        standVoiceLines = _audioConfig.lilyVoiceLinesStandPaths;

        _blackjackService.CardRequestedClient += OnCardRequested;
        _blackjackService.EndTurnRequestedClient += OnEndTurnRequested;
    }

    private void OnEndTurnRequested(object sender, EndTurnRequestedEventArgs e)
    {
        if(IsOwner)
            RequestVoiceLineServerRpc(standVoiceLines, transform.position);
    }

    private void OnCardRequested(object sender, CardRequestedEventArgs e)
    {
        if(IsOwner)
            RequestVoiceLineServerRpc(hitVoiceLines, transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestVoiceLineServerRpc(List<string> voicelines, Vector3 position)
    {
        var line = voicelines[Random.Range(0, voicelines.Count)];
        _audioService.OnPlaySoundAtPosition(position, line);
    }

    private void OnDestroy()
    {
        _blackjackService.CardRequestedClient -= OnCardRequested;
        _blackjackService.EndTurnRequestedClient -= OnEndTurnRequested;
    }
}
