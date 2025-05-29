using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialAudioManager : MonoBehaviour
{
    public static TutorialAudioManager Instance;

    private void Start()
    {
        if (Instance != null) Destroy(Instance);
        Instance = this;
    }

    public void OnPlaySoundAtPosition(Vector3 position, string audioPath)
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
}
