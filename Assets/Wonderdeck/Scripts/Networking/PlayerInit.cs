using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class PlayerInit : NetworkBehaviour
{
    [SerializeField] private Material firstPlayerMaterial;
    [SerializeField] private Material secondPlayerMaterial;
    
    [FormerlySerializedAs("cameraObject")] [SerializeField] private GameObject cinemachineVirtualCameraObject;
    [SerializeField] private GameObject cameraGameObject;
    [SerializeField] private CinemachineBrain _cinemachineBrain;


    [SerializeField] private Animator animator;
    


    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X"; 
    const string yAxis = "Mouse Y";
    
    
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
    

    [SerializeField] float sensitivity = 2f;

    private INetworkingService _networkingService;

    [Inject]
    private void ResolveDependencies(INetworkingService networkingService)
    {
        _networkingService = networkingService;
    }


    public override void OnStartClient()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (IsOwner)
            _networkingService.SetMyPlayer(gameObject);
        else
        {
            cinemachineVirtualCameraObject.GetComponent<CinemachineVirtualCamera>().enabled = false;
            _cinemachineBrain.enabled = false;
            cameraGameObject.GetComponent<AudioListener>().enabled = false;
            cameraGameObject.GetComponent<Camera>().enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
    }

    private Quaternion xQuat;
    private Quaternion yQuat;
    private void LateUpdate()
    {
        if (!IsOwner)
            return;
        cinemachineVirtualCameraObject.transform.localRotation = xQuat * yQuat;
    }

    private void OnDestroy()
    {
        _networkingService.SetMyPlayer(null);
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        cinemachineVirtualCameraObject.GetComponent<CinemachineVirtualCamera>().enabled = IsOwner;
        _cinemachineBrain.enabled = IsOwner;
        cameraGameObject.GetComponent<AudioListener>().enabled = IsOwner;
        cameraGameObject.GetComponent<Camera>().enabled = IsOwner;
        _networkingService.SetMyPlayer(gameObject);
    }
}