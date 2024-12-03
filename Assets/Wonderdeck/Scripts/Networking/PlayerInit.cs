using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerInit : NetworkBehaviour
{
    [SerializeField] private Material firstPlayerMaterial;
    [SerializeField] private Material secondPlayerMaterial;
    [SerializeField] private GameObject cameraObject;

    
    public override void OnStartClient()
    {
        if (IsOwner)
            return;
        cameraObject.SetActive(false);

    }
}
