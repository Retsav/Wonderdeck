using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    private NetworkManager _networkManager;
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private List<Transform> spawnPoints;
    private int _spawnIndex = 0;
    
    

    private void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            NetworkManagerExtensions.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        _networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoaded;
    }

    private void OnClientLoaded(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        SpawnPlayer(conn, spawnPoints[_spawnIndex]);
        _spawnIndex++;
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
        if(_networkManager != null) _networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoaded;
    }
}
