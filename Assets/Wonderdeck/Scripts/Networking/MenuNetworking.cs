using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    
    
    [Header("Side Popup Content")] 
    [SerializeField] private CanvasGroup sideBarCanvasGroup;
    [SerializeField] private TextMeshProUGUI sidebarTitleLabel;
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private Button confirmButton;

    private Tugboat _tb;
    private const string DEBUG_DEFAULT_ADDRESS = "26.39.26.158";

    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;
    
    private void Start()
    {
        _tb = InstanceFinder.TransportManager.GetTransport<Tugboat>();
        addressInputField.text = DEBUG_DEFAULT_ADDRESS;
        startHostButton.onClick.RemoveAllListeners();
        startClientButton.onClick.RemoveAllListeners();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        startClientButton.onClick.AddListener(InitClientSideBar);
        startHostButton.onClick.AddListener(InitHostSideBar);
        HideSideBar();
        InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
        InstanceFinder.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
    }

    private void HideSideBar(bool animated = false)
    {
        sideBarCanvasGroup.interactable = false;
        sideBarCanvasGroup.blocksRaycasts = false;
        if (animated)
            sideBarCanvasGroup.DOFade(0f, 0.3f);
        else
            sideBarCanvasGroup.alpha = 0f;
    }

    private void ShowSideBar(bool animated = false)
    {
        sideBarCanvasGroup.interactable = true;
        sideBarCanvasGroup.blocksRaycasts = true;
        if (animated)
            sideBarCanvasGroup.DOFade(1f, 0.3f);
        else
            sideBarCanvasGroup.alpha = 1f;
    }

    private void InitClientSideBar()
    {
        sidebarTitleLabel.text = "JOIN";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnStartClient);
        if(sideBarCanvasGroup.alpha < .9f)
            ShowSideBar(true);
    }
    
    private void InitHostSideBar()
    {
        sidebarTitleLabel.text = "HOST";
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnStartHost);
        if(sideBarCanvasGroup.alpha < .9f)
            ShowSideBar(true);
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
        _tb.SetClientAddress(addressInputField.text);
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
        _tb.SetClientAddress(addressInputField.text);
        if (_clientState != LocalConnectionState.Stopped)
            InstanceFinder.NetworkManager.ClientManager.StopConnection();
        else
            InstanceFinder.NetworkManager.ClientManager.StartConnection();
            
    }
}
