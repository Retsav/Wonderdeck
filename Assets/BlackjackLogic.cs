using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class BlackjackLogic : NetworkBehaviour
{
   private List<CardSO> _orginalDeck;
   private List<CardSO> _deck;

   private DeckConfig _deckConfig;

   private NetworkManager _networkManager;
   
   public override void OnStartClient()
   {
      _deckConfig = DebugConfigLoader.Instance.GetConfig<DeckConfig>();
      _orginalDeck = _deckConfig.cards;
      _networkManager = InstanceFinder.NetworkManager;
      StartGameServerRpc();
      /*if (IsAuthorizedToStart(_networkManager.ClientManager.Connection) && AreBothPlayersConnected())
         StartGameServerRpc();
      else if (IsAuthorizedToStart(_networkManager.ClientManager.Connection) && !AreBothPlayersConnected())
         _networkManager.ServerManager.OnRemoteConnectionState += OnPlayerConnectionStateChanged;*/
   }

   private void OnPlayerConnectionStateChanged(NetworkConnection conn, RemoteConnectionStateArgs args)
   {
      if (args.ConnectionState == RemoteConnectionState.Stopped)
      {
         Debug.Log($"Player {conn.ClientId} disconnected.");
      } else if (args.ConnectionState == RemoteConnectionState.Started)
      {
         Debug.Log($"Player {conn.ClientId} connected. Attempting to start the game");
         if (AreBothPlayersConnected()) StartGameServerRpc();
      }
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Space))
      {
         StartGameServerRpc();
      }
   }

   [ServerRpc(RequireOwnership = false)]
   private void StartGameServerRpc()
   {
      if(AreBothPlayersConnected())
         StartGameObserverRpc();
   }

   [ObserversRpc]
   private void StartGameObserverRpc()
   {
      Debug.Log("Game started success!");
   }

   private bool IsAuthorizedToStart(NetworkConnection conn) => conn.ClientId == 0;

   private bool AreBothPlayersConnected()
   {
      int connectedPlayers = _networkManager.ServerManager.Clients.Count;
      return connectedPlayers >= 2;
   }

   private void OnDestroy()
   {
      if(_networkManager != null) _networkManager.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionStateChanged;
   }
}
