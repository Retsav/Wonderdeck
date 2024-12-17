using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public class PlayerService : IPlayersService
{
    public event EventHandler<NetworkConnection> PlayerConnectedEventHandler;
    public event EventHandler<NetworkConnection> PlayerDisconnectedEventHandler;
    public void OnPlayerDisconnected(NetworkConnection conn) => PlayerDisconnectedEventHandler?.Invoke(this, conn);
    public void OnPlayerConnected(NetworkConnection conn) => PlayerConnectedEventHandler?.Invoke(this, conn);
}
