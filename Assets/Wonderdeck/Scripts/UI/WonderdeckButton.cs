using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WonderdeckButton : Button
{
    private AudioSource _audioSource;
    private const string BUTTON_CLICK_PATH = "Audio/Audio_ClickUI";
    
    
    protected override void Start()
    {
        base.Start();
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.clip = Resources.Load<AudioClip>(BUTTON_CLICK_PATH);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        _audioSource.Play();
    }
}
