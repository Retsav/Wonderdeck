using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputService : IInputService
{
    public void OnInputRegistered(object sender) => OnInputRegisteredEvent?.Invoke(sender, EventArgs.Empty);
    public event EventHandler OnInputRegisteredEvent;
}
