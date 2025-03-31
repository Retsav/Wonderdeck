using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputService
{
    public void OnInputRegistered(object sender);
    public event EventHandler OnInputRegisteredEvent;
}
