using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuNetworking : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI serverStateLabel;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private int _connectedPlayers;
    
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;
    
    private void Start()
    {
        startHostButton.onClick.RemoveAllListeners();
        startClientButton.onClick.RemoveAllListeners();
        startClientButton.onClick.AddListener(OnStartClient);
        startHostButton.onClick.AddListener(OnStartHost);
        InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        InstanceFinder.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.NetworkManager == null) return;
        InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        InstanceFinder.NetworkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj) => _serverState = obj.ConnectionState;
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj) => _clientState = obj.ConnectionState;

    private void OnStartHost()
    {
        if (_serverState == LocalConnectionState.Stopped)
        {
            InstanceFinder.NetworkManager.ServerManager.StartConnection();
            InstanceFinder.NetworkManager.ServerManager.OnAuthenticationResult += OnAuthenticationResult;
            InstanceFinder.NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        else
        {
            InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
            InstanceFinder.NetworkManager.ServerManager.OnAuthenticationResult -= OnAuthenticationResult;
            InstanceFinder.NetworkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }
            
    }
    

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs state)
    {
        if (state.ConnectionState != RemoteConnectionState.Stopped)
            return;
        _connectedPlayers--;
    }

    private void OnAuthenticationResult(NetworkConnection conn, bool res)
    {
        if (!res) return;
        _connectedPlayers++;
        CheckForSceneTransition();
    }

    private void CheckForSceneTransition()
    {
        if (_connectedPlayers < 2)
            return;
        InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        InstanceFinder.NetworkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        InstanceFinder.NetworkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        InstanceFinder.NetworkManager.ServerManager.OnAuthenticationResult -= OnAuthenticationResult;
        startHostButton.onClick.RemoveAllListeners();
        startClientButton.onClick.RemoveAllListeners();
        _connectedPlayers = 0;
        SceneLoadData sld = new SceneLoadData(new string[] {"DebugScene 1", "DebugUISCene"});
        sld.ReplaceScenes = ReplaceOption.All;
        sld.PreferredActiveScene = new PreferredScene(sld.SceneLookupDatas[0]);
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }

    private void OnStartClient()
    {
        if (_clientState != LocalConnectionState.Stopped)
            InstanceFinder.NetworkManager.ClientManager.StopConnection();
        else
            InstanceFinder.NetworkManager.ClientManager.StartConnection();
            
    }
}
