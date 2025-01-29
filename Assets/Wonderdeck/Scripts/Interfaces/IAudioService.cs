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
    public void OnPlaySoundLocal(Vector3 position, string audioPath);
}



public class PlaySoundAtPositionEventArgs : EventArgs
{
    public bool IsGlobal;
    public Vector3 Position;
    public string AudioPath;

    public PlaySoundAtPositionEventArgs(Vector3 position, string audioPath, bool isGlobal)
    {
        Position = position;
        AudioPath = audioPath;
        IsGlobal = isGlobal;
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