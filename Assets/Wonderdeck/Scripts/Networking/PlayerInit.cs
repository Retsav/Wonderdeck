using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerInit : NetworkBehaviour
{
    [SerializeField] private Material firstPlayerMaterial;
    [SerializeField] private Material secondPlayerMaterial;
    [SerializeField] private GameObject cameraObject;


    [SerializeField] private Animator animator;
    
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 60f;

    private float pitch = 0f;
    private float yaw = 0f;    
    
    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X"; 
    const string yAxis = "Mouse Y";
    
    
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform lookAtTransform;
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
    

    [SerializeField] float sensitivity = 2f;


    public override void OnStartClient()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (IsOwner)
        {
            pitch = 0f;
            yaw = 0f;
        }
        else
        {
            cameraObject.GetComponent<Camera>().enabled = false;
            cameraObject.GetComponent<AudioListener>().enabled = false;
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
        cameraObject.transform.localRotation = xQuat * yQuat;
    }
    
    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        cameraObject.GetComponent<Camera>().enabled = IsOwner;
        cameraObject.GetComponent<AudioListener>().enabled = IsOwner;
    }
}