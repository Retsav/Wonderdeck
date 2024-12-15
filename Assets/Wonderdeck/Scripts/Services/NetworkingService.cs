using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public class NetworkingService : INetworkingService
{
    public PlayerType GetPlayerType(NetworkConnection conn)
    {
        if (conn.IsHost) return PlayerType.Player1;
        return PlayerType.Player2;
    }
}
