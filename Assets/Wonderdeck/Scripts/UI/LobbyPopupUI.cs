using System.Collections;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyPopupUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI firstPlayerNicknameLabel;
    [SerializeField] private TextMeshProUGUI secondPlayerNicknameLabel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button exitLobbyButton;

    // Host jest zawsze połączony, dlatego zaczynamy od 1.
    private int _connectedPlayers = 1;

    private void ResetSecondPlayerNickname() => secondPlayerNicknameLabel.text = "";

    [ObserversRpc(ExcludeServer = true)]
    private void RequestNicknameFromPlayerObserverRpc()
    {
        var nickname = PlayerPrefs.GetString("Nickname");
        SendNicknameServerRpc(nickname);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendNicknameServerRpc(string nickname)
    {
        var hostNickname = PlayerPrefs.GetString("Nickname");
        SetNicknamesObserverRpc(hostNickname, nickname);
    }

    [ObserversRpc]
    private void SetNicknamesObserverRpc(string hostNickname, string nickname)
    {
        firstPlayerNicknameLabel.text = hostNickname;
        secondPlayerNicknameLabel.text = nickname;
    }

    public override void OnStartClient()
    {
        Init();
    }


    public void Init()
    {
        firstPlayerNicknameLabel.text = "";
        secondPlayerNicknameLabel.text = "";
        
        exitLobbyButton.onClick.RemoveAllListeners();
        exitLobbyButton.onClick.AddListener(StopConnection);
        
        if (NetworkManager.ClientManager.Connection.IsHost)
        {
            var nickname = PlayerPrefs.GetString("Nickname");
            firstPlayerNicknameLabel.text = nickname;
            startGameButton.onClick.RemoveAllListeners();
            startGameButton.onClick.AddListener(StartGame);
            startGameButton.interactable = (_connectedPlayers >= 2);
            NetworkManager.ServerManager.OnAuthenticationResult += OnPlayerAuthenticated;
            NetworkManager.ServerManager.OnRemoteConnectionState += OnPlayerDisconnected;
            RequestServerNicknameServerRpc();
        }
        else
        {
            NetworkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.ClientManager.Connection.IsHost)
        {
            InstanceFinder.NetworkManager.ServerManager.OnAuthenticationResult -= OnPlayerAuthenticated;
            InstanceFinder.NetworkManager.ServerManager.OnRemoteConnectionState -= OnPlayerDisconnected;
        }
        else
        {
            InstanceFinder.NetworkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }


    private void OnPlayerAuthenticated(NetworkConnection conn, bool result)
    {
        if (!result) return;
        _connectedPlayers++;
        startGameButton.interactable = (_connectedPlayers >= 2);
        StartCoroutine(RequestNicknameDelay());
    }

    private IEnumerator RequestNicknameDelay()
    {
        yield return new WaitForSeconds(0.2f);
        RequestNicknameFromPlayerObserverRpc();
    }
    
    private void OnPlayerDisconnected(NetworkConnection conn, RemoteConnectionStateArgs state)
    {
        if (state.ConnectionState != RemoteConnectionState.Stopped)
            return;
        _connectedPlayers--;
        ResetSecondPlayerNickname();
        startGameButton.interactable = (_connectedPlayers >= 2);
    }


    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped) StopConnection();
    }

    private void StartGame()
    {
        if (_connectedPlayers < 2)
            return;
        SceneLoadData sld = new SceneLoadData(new string[] { "DebugScene 1", "DebugUISCene" });
        sld.ReplaceScenes = ReplaceOption.All;
        sld.PreferredActiveScene = new PreferredScene(sld.SceneLookupDatas[0]);
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }

    private void StopConnection()
    {
        if (NetworkManager.ClientManager.Connection.IsHost)
            NetworkManager.ServerManager.StopConnection(true);
        else
            NetworkManager.ClientManager.StopConnection();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestServerNicknameServerRpc()
    {
        var serverNickname = PlayerPrefs.GetString("Nickname");
        SendServerNicknameObserverRpc(serverNickname);
    }

    [ObserversRpc]
    private void SendServerNicknameObserverRpc(string serverNickname)
    {
        firstPlayerNicknameLabel.text = serverNickname;
    }



    public void OnHostDisconnected()
    {
        firstPlayerNicknameLabel.text = "";
        secondPlayerNicknameLabel.text = "";
    }
}
