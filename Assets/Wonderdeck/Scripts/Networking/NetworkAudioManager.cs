using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class NetworkAudioManager : NetworkBehaviour
{
    private IAudioService _audioService;

    
    
    [Inject]
    private void ResolveDependencies(IAudioService audioService)
    {
        _audioService = audioService;
    }

    public override void OnStartClient()
    {
        InitAudioSources();
        _audioService.PlaySoundAtPositionEvent += OnPlaySoundAtPositionLocal;
        if (!ClientManager.Connection.IsHost) return;
        _audioService.PlaySoundAtPositionEvent += OnPlaySoundAtPositionGlobal;
        _audioService.PlaySoundEvent += OnPlaySound;
    }

    private void OnPlaySoundAtPositionLocal(object sender, PlaySoundAtPositionEventArgs e)
    {
        if (e.IsGlobal) return;
        AudioSource source = GetFreeAudioSource();
        if (source == null)
        {
            Debug.LogWarning("Audio Pool Exhausted!");
            return;
        }
        source.gameObject.SetActive(true);
        source.transform.position = e.Position;
        source.clip = Resources.Load<AudioClip>(e.AudioPath);
        source.Play();
        StartCoroutine(ReleaseAudioSourceCoroutine(source));
    }

    private void InitAudioSources()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out AudioSource source)) 
                source.clip = null;
            else
                Debug.LogWarning($"Child in NetworkAudioManager does not have AudioSource component.");
            child.gameObject.SetActive(false);
        }
    }

    private void OnPlaySoundAtPositionGlobal(object sender, PlaySoundAtPositionEventArgs e)
    {
        if (!ClientManager.Connection.IsHost || !e.IsGlobal) return;
        PlaySoundAtPositionObserverRpc(e.Position, e.AudioPath);
    }

    [ObserversRpc]
    private void PlaySoundAtPositionObserverRpc(Vector3 position, string audioPath)
    {
        AudioSource source = GetFreeAudioSource();
        if (source == null)
        {
            Debug.LogWarning("Audio Pool Exhausted!");
            return;
        }
        source.gameObject.SetActive(true);
        source.transform.position = position;
        source.clip = Resources.Load<AudioClip>(audioPath);
        source.Play();
        StartCoroutine(ReleaseAudioSourceCoroutine(source));
    }

    private IEnumerator ReleaseAudioSourceCoroutine(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        source.clip = null;
        source.gameObject.SetActive(false);
    }

    private AudioSource GetFreeAudioSource()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out AudioSource source))
            {
                if (source.clip != null) continue;
                return source;
            }
            else
                Debug.LogWarning($"Child in NetworkAudioManager does not have AudioSource component.");
        }
        return null;
    }



    private void OnPlaySound(object sender, PlaySoundEventArgs e)
    {
        throw new System.NotImplementedException();
    }
    
    
    private void OnDestroy()
    {
        if (ClientManager == null) return;
        _audioService.PlaySoundAtPositionEvent -= OnPlaySoundAtPositionLocal;
        if (!ClientManager.Connection.IsHost) return;
        _audioService.PlaySoundEvent -= OnPlaySound;
        _audioService.PlaySoundAtPositionEvent -= OnPlaySoundAtPositionGlobal;
    }
}
