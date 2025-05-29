using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutorialPlayerInit : MonoBehaviour
{
    public static TutorialPlayerInit Instance;
    
    [SerializeField] private Material firstPlayerMaterial;
    [SerializeField] private Material secondPlayerMaterial;
    
    [SerializeField] private GameObject cinemachineVirtualCameraObject;
    [SerializeField] private GameObject cameraGameObject;
    [SerializeField] private CinemachineBrain _cinemachineBrain;

    [SerializeField] private MusicManager musicManager;
    

    [SerializeField] private Animator animator;
    


    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X"; 
    const string yAxis = "Mouse Y";
    
    private Quaternion xQuat;
    private Quaternion yQuat;
    
    
    [Range(0f, 90f)][SerializeField] float yRotationLimit = 88f;
    

    [SerializeField] float sensitivity = 2f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        musicManager.StartMusic();
        if (Instance != null) Destroy(Instance);
        Instance = this;
    }

    private void Update()
    {
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);
        xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
    }
    
    
    private void LateUpdate()
    {
        cinemachineVirtualCameraObject.transform.localRotation = xQuat * yQuat;
    }
}
