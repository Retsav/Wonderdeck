using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioService
{
    public event EventHandler<PlaySoundAtPositionEventArgs> PlaySoundAtPositionEvent; 
    public void OnPlaySoundAtPosition(Vector3 position, string audioPath);
    public event EventHandler<PlaySoundEventArgs> PlaySoundEvent;
    public void OnPlaySound(string audioPath);
}



public class PlaySoundAtPositionEventArgs : EventArgs
{
    public Vector3 Position;
    public string AudioPath;

    public PlaySoundAtPositionEventArgs(Vector3 position, string audioPath)
    {
        Position = position;
        AudioPath = audioPath;
    }
}

public class PlaySoundEventArgs : EventArgs
{
    public string AudioPath;

    public PlaySoundEventArgs(string audioPath)
    {
        AudioPath = audioPath;
    }
}