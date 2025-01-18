using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    private NetworkManager _networkManager;
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private List<Transform> spawnPoints;
    private int _spawnIndex = 0;

    private HashSet<NetworkConnection> _handledConnections = new HashSet<NetworkConnection>();

    private void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            NetworkManagerExtensions.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        _networkManager.SceneManager.OnClientPresenceChangeEnd += OnClientLoaded;
    }

    private void OnClientLoaded(ClientPresenceChangeEventArgs obj)
    {
        if (!obj.Added) return;
        if(!_networkManager.ClientManager.Connection.IsHost)
            return;
        if (_spawnIndex > spawnPoints.Count - 1) return;
        if (_handledConnections.Contains(obj.Connection)) return;
        SpawnPlayer(obj.Connection, spawnPoints[_spawnIndex]);
        _handledConnections.Add(obj.Connection);
        _spawnIndex++;
    }

    private void OnClientLoaded(NetworkConnection conn, bool asServer)
    {

    }
    
    private void SpawnPlayer(NetworkConnection conn, Transform objectTransform)
    {
        NetworkObject playerInstance = Instantiate(playerPrefab);
        if (playerInstance != null)
        {
            var playerTransform = playerInstance.transform;
            playerTransform.position = objectTransform.position;
            playerTransform.rotation = objectTransform.rotation;
            _networkManager.ServerManager.Spawn(playerInstance, conn);
        }
        else
            Debug.LogError("Spawned PlayerInstance is null");
    }

    private void OnDestroy()
    {
        if(_networkManager != null) _networkManager.SceneManager.OnClientPresenceChangeEnd -= OnClientLoaded;
    }
}
