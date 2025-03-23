using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Object;
using UnityEngine;

public class PlayerCameraRaycaster : NetworkBehaviour
{
    [SerializeField] private float rayDistance;
    [SerializeField] private CinemachineVirtualCamera camera;
    

    private ICameraInteractable _lastInteractable;


    public override void OnStartClient()
    {
        if(!IsOwner)
            Destroy(this);
    }

    private void Update()
    {
        var cameraTransform = camera.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            var interactable = hit.collider.GetComponent<ICameraInteractable>();
            if (interactable != null)
            {
                if (_lastInteractable == interactable) return;
                _lastInteractable?.OnCameraExit();
                _lastInteractable = interactable;
                interactable.OnCameraOver();
            }
            else
                ClearInteractable();
        }
        else
            ClearInteractable();
    }

    private void ClearInteractable()
    {
        if (_lastInteractable == null) return;
        _lastInteractable.OnCameraExit();
        _lastInteractable = null;
    }
}
