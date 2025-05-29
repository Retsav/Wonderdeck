using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Zenject;

public class LookAtTransformHandler : MonoBehaviour
{
    [SerializeField] private GameObject cameraGameObject;

    private CinemachineVirtualCamera _camera;


    private IInventoryService _inventoryService;


    [Inject]
    private void ResolveDependencies(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }
    
    private void Start()
    {
        _camera = cameraGameObject.GetComponent<CinemachineVirtualCamera>();
    }


    private void Update()
    {
        if(!_camera.enabled) return;
        if (_inventoryService.IsInventoryOpened) return;
        float distanceInFrontOfCamera = 1f;

        // Calculate the position
        Vector3 positionInFrontOfCamera = _camera.transform.position + _camera.transform.forward * distanceInFrontOfCamera;
        
        transform.position = positionInFrontOfCamera;
        transform.rotation = cameraGameObject.transform.rotation;
    }
}
