using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnvironmentService
{
    public void ChangeScenery(string key);
    public void OnEarlySceneryChange(string key);
    public void OnSceneryChange(string key);
    public void OnLateSceneryChange(string key);
    public event EventHandler<SceneryChangedEventArgs> EarlySceneryChangedEvent;
    public event EventHandler<SceneryChangedEventArgs> SceneryChangedEvent;
    public event EventHandler<SceneryChangedEventArgs> LateSceneryChangedEvent;
}


public class SceneryChangedEventArgs : EventArgs
{
    public string Key;
    public SceneryChangedEventArgs(string key) => Key = key;
}
