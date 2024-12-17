using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;
using Zenject;

public class DebugConnectionTest : NetworkBehaviour
{
    private IPlayersService _playersService;

    [Inject]
    private void ResolveDependencies(IPlayersService playersService)
    {
        _playersService = playersService;
    }

    private void Start()
    {
        
    }
}
