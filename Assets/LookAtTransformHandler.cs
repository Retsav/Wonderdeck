using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class LookAtTransformHandler : MonoBehaviour
{
    [SerializeField] private GameObject cameraGameObject;

    private CinemachineVirtualCamera _camera;


    private void Start()
    {
        _camera = cameraGameObject.GetComponent<CinemachineVirtualCamera>();
    }


    private void Update()
    {
        if(!_camera.enabled) return;
        float distanceInFrontOfCamera = 1f;

        // Calculate the position
        Vector3 positionInFrontOfCamera = _camera.transform.position + _camera.transform.forward * distanceInFrontOfCamera;
        
        transform.position = positionInFrontOfCamera;
        transform.rotation = cameraGameObject.transform.rotation;
    }
}
