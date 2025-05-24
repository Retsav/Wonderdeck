using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> musicList;


    public void StartMusic()
    {
        var randomClip = Random.Range(0, musicList.Count - 1);
        audioSource.clip = musicList[randomClip];
        audioSource.Play();
        StartCoroutine(PlayNextAudioSource());
    }
    
    
    private IEnumerator PlayNextAudioSource()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        audioSource.clip = null;
        StartMusic();
    }

    private void OnDestroy()
    {
        audioSource.Stop();
        StopCoroutine(PlayNextAudioSource());
    }
}
