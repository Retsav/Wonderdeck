using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class PlayerInit : NetworkBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material firstPlayerMaterial;
    [SerializeField] private Material secondPlayerMaterial;
    [SerializeField] private List<SkinnedMeshRenderer> aliceModel;
    [SerializeField] private List<SkinnedMeshRenderer> queenModel;
    

    [Header("Camera References")]
    [SerializeField] private GameObject cinemachineVirtualCameraObject;
    [SerializeField] private GameObject cameraGameObject;
    [SerializeField] private CinemachineBrain _cinemachineBrain;
    [SerializeField] private MusicManager musicManager;



    [Header("Mouse Settings")]
    const string xAxis = "Mouse X";
    const string yAxis = "Mouse Y";
    [SerializeField, Range(0f, 180f)]
    private float yawLimit = 90f;               // maks. odchył w lewo/prawo
    [SerializeField, Range(0f, 90f)]
    private float pitchLimit = 88f;             // maks. odchył w górę/dół
    [SerializeField] private float sensitivity = 2f;

    private Vector2 rotation = Vector2.zero;
    private Quaternion xQuat;
    private Quaternion yQuat;

    private INetworkingService _networkingService;
    private IInventoryService _inventoryService;

    [Inject]
    private void ResolveDependencies(INetworkingService networkingService, IInventoryService inventoryService)
    {
        _networkingService = networkingService;
        _inventoryService = inventoryService;
    }

    public override void OnStartClient()
    {
        // zablokuj kursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach (var sm in aliceModel) sm.enabled = false;
        foreach (var sm in queenModel) sm.enabled = false;
        {
            
        }
        if (IsOwner)
        {
            _networkingService.SetMyPlayer(gameObject);
            musicManager.StartMusic();
            StartCoroutine(LoadMyCharacterCoroutine());
        }
        else
        {
            cinemachineVirtualCameraObject.GetComponent<CinemachineVirtualCamera>().enabled = false;
            _cinemachineBrain.enabled = false;
            cameraGameObject.GetComponent<AudioListener>().enabled = false;
            cameraGameObject.GetComponent<Camera>().enabled = false;
            StartCoroutine(LoadSecondCharacterCoroutine());
        }
    }

    IEnumerator LoadMyCharacterCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (NetworkManager.ClientManager.Connection.IsHost)
            foreach (var sm in aliceModel) sm.enabled = true;
        else
            foreach (var sm in queenModel) sm.enabled = true;
    }

    IEnumerator LoadSecondCharacterCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (NetworkManager.ClientManager.Connection.IsHost)
            foreach (var sm in queenModel) sm.enabled = true;
        else
            foreach (var sm in aliceModel) sm.enabled = true;
    }



    private void Update()
    {
        if (!IsOwner) return;
        if (_inventoryService.IsInventoryOpened) return;
        
        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;


        rotation.y = Mathf.Clamp(rotation.y, -pitchLimit, pitchLimit);
        rotation.x = Mathf.Clamp(rotation.x, -yawLimit, yawLimit);
        
        xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        cinemachineVirtualCameraObject.transform.localRotation = xQuat * yQuat;
    }

    private void OnDestroy()
    {

        _networkingService.SetMyPlayer(null);
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {

        var vcam = cinemachineVirtualCameraObject
                      .GetComponent<CinemachineVirtualCamera>();
        vcam.enabled = IsOwner;
        _cinemachineBrain.enabled = IsOwner;
        cameraGameObject.GetComponent<AudioListener>().enabled = IsOwner;
        cameraGameObject.GetComponent<Camera>().enabled = IsOwner;
        _networkingService.SetMyPlayer(gameObject);
    }
}
