using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.UI;

public class MenuNetworking : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;
    
    private void Start()
    {
        startHostButton.onClick.RemoveAllListeners();
        startClientButton.onClick.RemoveAllListeners();
        startClientButton.onClick.AddListener(OnStartClient);
        startHostButton.onClick.AddListener(OnStartHost);
        networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }

    private void OnDestroy()
    {
        networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj) => _serverState = obj.ConnectionState;
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj) => _clientState = obj.ConnectionState;

    private void OnStartHost()
    {
        if (_serverState == LocalConnectionState.Stopped)
        {
            networkManager.ServerManager.StartConnection();
            networkManager.ServerManager.OnAuthenticationResult += OnAuthenticationResult;
        }

        else
        {
            networkManager.ServerManager.StopConnection(true);
            networkManager.ServerManager.OnAuthenticationResult -= OnAuthenticationResult;
        }
            
    }

    private void OnAuthenticationResult(NetworkConnection conn, bool res)
    {
        Debug.Log($"Connection: {conn.ClientId} with result: {res}");
    }

    private void OnStartClient()
    {
        if (_clientState != LocalConnectionState.Stopped)
            networkManager.ClientManager.StopConnection();
        else
            networkManager.ClientManager.StartConnection();
    }
}
