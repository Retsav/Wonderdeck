using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WonderdeckButton : Button
{
    private AudioSource _audioSource;
    private const string BUTTON_CLICK_PATH = "Audio/Audio_ClickUI";

    [SerializeField] private List<Image> targetImages = new();
    [SerializeField] private List<Color> transitionColorsEnabled = new();
    [SerializeField] private List<Color> transitionColorsDisabled = new();

    [SerializeField] private Color enabledTextColor;
    [SerializeField] private Color disabledTextColor;


    private TextMeshProUGUI _buttonText;
    
    
    
    protected override void Start()
    {
        base.Start();
        _audioSource = !gameObject.TryGetComponent(out AudioSource src) ? gameObject.AddComponent<AudioSource>() : src;
        _buttonText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        _audioSource.playOnAwake = false;
        _audioSource.clip = Resources.Load<AudioClip>(BUTTON_CLICK_PATH);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        _audioSource.Play();
    }

    public void SetInteractable(bool isInteractable)
    {
        interactable = isInteractable;
        TransitionColors();
    }

    private void TransitionColors()
    {
        if (targetImages.Count <= 0)
            return;
        var transitionColors = interactable ? transitionColorsEnabled : transitionColorsDisabled;
        if (transitionColors.Count <= 0)
            return;
        int count = Math.Min(targetImages.Count, transitionColors.Count);
        for (int i = 0; i < count; i++) targetImages[i].DOColor(transitionColors[i], 0.3f);
        if (_buttonText == null)
            return;
        var textColorToTransition = interactable ? enabledTextColor : disabledTextColor;
        _buttonText.DOColor(textColorToTransition, 0.3f);
    }
}
