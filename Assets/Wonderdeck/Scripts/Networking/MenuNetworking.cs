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
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuNetworking : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI serverStateLabel;
    
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    
    
    [Header("Side Popup Content")] 
    [SerializeField] private CanvasGroup sideBarCanvasGroup;
    [SerializeField] private TextMeshProUGUI sidebarTitleLabel;
    [SerializeField] private TMP_InputField addressInputField;
    [SerializeField] private Button confirmButton;

    private Tugboat _tb;
    private const string DEBUG_DEFAULT_ADDRESS = "127.0.0.1";

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

    private void OnDisable()
    {
        Debug.Log("Debug");
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

    private void InitClientSideBar() => InitSideBar("JOIN", OnStartClient);
    private void InitHostSideBar() => InitSideBar("HOST", OnStartHost);


    private void InitSideBar(string titleLabelText, UnityAction call)
    {
        sidebarTitleLabel.text = titleLabelText;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(call);
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
    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _clientState = obj.ConnectionState;
    }

    private void OnStartHost()
    {
        _tb.SetClientAddress(addressInputField.text);
        if (_serverState == LocalConnectionState.Stopped)
        {
            InstanceFinder.NetworkManager.ServerManager.StartConnection();
        }
        else
        {
            InstanceFinder.NetworkManager.ServerManager.StopConnection(true);
        }
    }

    
    private void OnStartClient()
    {
        _tb.SetClientAddress(addressInputField.text);
        if (_clientState != LocalConnectionState.Stopped)
            InstanceFinder.NetworkManager.ClientManager.StopConnection();
        else
        {
            InstanceFinder.NetworkManager.ClientManager.StartConnection();
            InstanceFinder.ClientManager.OnClientConnectionState += OnServerStartedForLobby;
        }
            
            
    }

    private void OnServerStartedForLobby(ClientConnectionStateArgs args)
    {
        StartCoroutine(TryToSwitchToLobby(args));
    }

    private IEnumerator TryToSwitchToLobby(ClientConnectionStateArgs args)
    {
        yield return new WaitForSeconds(2f);
        if (!InstanceFinder.NetworkManager.ClientManager.Connection.IsHost)
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= OnServerStartedForLobby;
            yield break;
        }   
        if (args.ConnectionState != LocalConnectionState.Started) yield break;
        InstanceFinder.ClientManager.OnClientConnectionState -= OnServerStartedForLobby;
        SceneLoadData sld = new SceneLoadData(new string[] { "Lobby" });
        sld.ReplaceScenes = ReplaceOption.All;
        sld.PreferredActiveScene = new PreferredScene(sld.SceneLookupDatas[0]);
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }
}
