using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public class NetworkingService : INetworkingService
{
    private GameObject _myPlayer;

    public void SetMyPlayer(GameObject player)
    {
        _myPlayer = player;
    }
    
    public PlayerType GetPlayerType(NetworkConnection conn)
    {
        if (conn.IsHost) return PlayerType.Player1;
        return PlayerType.Player2;
    }

    public GameObject GetMyPlayer() => _myPlayer;
}
