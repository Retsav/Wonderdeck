using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface INetworkingService
{
    public PlayerType GetPlayerType(NetworkConnection conn);
}
