using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public class NetworkingService : INetworkingService
{
    public string FirstPlayerNickname { get; set; }
    public string SecondPlayerNickname { get; set; }
    
    
    private GameObject _myPlayer;

    public void SetMyPlayer(GameObject player) => _myPlayer = player;


    public void RegisterNickname(PlayerType playerType, string nickname)
    {
        if (playerType == PlayerType.Player1)
            FirstPlayerNickname = nickname;
        else
            SecondPlayerNickname = nickname;
    }

    public PlayerType GetPlayerType(NetworkConnection conn)
    {
        if (conn.IsHost) return PlayerType.Player1;
        return PlayerType.Player2;
    }

    public GameObject GetMyPlayer() => _myPlayer;
}
