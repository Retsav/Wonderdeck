using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioService : IAudioService
{
    public event EventHandler<PlaySoundAtPositionEventArgs> PlaySoundAtPositionEvent;
    public void OnPlaySoundAtPosition(Vector3 position, string audioPath) => PlaySoundAtPositionEvent?.Invoke(this, new PlaySoundAtPositionEventArgs(position, audioPath, true));
    public event EventHandler<PlaySoundEventArgs> PlaySoundEvent;
    public void OnPlaySound(string audioPath) => PlaySoundEvent?.Invoke(this, new PlaySoundEventArgs(audioPath));
    public void OnPlaySoundLocal(Vector3 position, string audioPath) => PlaySoundAtPositionEvent?.Invoke(this, new PlaySoundAtPositionEventArgs(position, audioPath, false));
}
