using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitHandler : MonoBehaviour
{
    private NetworkManager _networkManager;
    private void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnClientConnectionState(ClientConnectionStateArgs state)
    {
        if (state.ConnectionState == LocalConnectionState.Stopped)
        {
            InstanceFinder.NetworkManager.ClientManager.StopConnection();
            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            Debug.LogWarning($"Lost connection to the host. Returning to the Main Menu");
            SceneManager.LoadScene("Menu");
        }
    }
    

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (InstanceFinder.NetworkManager.ClientManager.Connection.IsHost)
            InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
        //InstanceFinder.NetworkManager.ClientManager.StopConnection();
    }
}
