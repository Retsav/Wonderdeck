using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentService : IEnvironmentService
{
    public void ChangeScenery(string key)
    {
        OnEarlySceneryChange(key);
        OnSceneryChange(key);
    }

    public void OnEarlySceneryChange(string key) => EarlySceneryChangedEvent?.Invoke(this, new SceneryChangedEventArgs(key));
    public void OnSceneryChange(string key) => SceneryChangedEvent?.Invoke(this, new SceneryChangedEventArgs(key));
    public void OnLateSceneryChange(string key) => LateSceneryChangedEvent?.Invoke(this, new SceneryChangedEventArgs(key));

    public event EventHandler<SceneryChangedEventArgs> EarlySceneryChangedEvent;
    public event EventHandler<SceneryChangedEventArgs> SceneryChangedEvent;
    public event EventHandler<SceneryChangedEventArgs> LateSceneryChangedEvent;
}
