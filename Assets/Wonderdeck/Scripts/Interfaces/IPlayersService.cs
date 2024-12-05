using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface IPlayersService
{
    public event EventHandler<NetworkConnection> PlayerConnectedEventHandler;
    public event EventHandler<NetworkConnection> PlayerDisconnectedEventHandler;
    public void OnPlayerDisconnected(NetworkConnection conn);
    public void OnPlayerConnected(NetworkConnection conn);
}
