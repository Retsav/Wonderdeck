using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

public interface INetworkingService
{
    public string FirstPlayerNickname { get; set; }
    public string SecondPlayerNickname { get; set; }

    public void RegisterNickname(PlayerType playerType, string nickname);
    public PlayerType GetPlayerType(NetworkConnection conn);
    public GameObject GetMyPlayer();
    public void SetMyPlayer(GameObject playerObject);
}
