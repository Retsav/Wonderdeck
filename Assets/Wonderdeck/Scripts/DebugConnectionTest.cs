using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class DebugConnectionTest : NetworkBehaviour
{
    public override void OnStartNetwork()
    {
        SendMessageServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMessageServerRpc()
    {
        Debug.Log("1");
        SendMessageObserverRpc();
    }

    [ObserversRpc]
    private void SendMessageObserverRpc()
    {
        Debug.Log("2");
    }
}
