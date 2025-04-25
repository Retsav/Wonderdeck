using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutorialCameraRaycaster : MonoBehaviour
{
    [SerializeField] private float rayDistance;
    [SerializeField] private CinemachineVirtualCamera vCamera;
    

    private ICameraInteractable _lastInteractable;


    private void Update()
    {
        var cameraTransform = vCamera.transform;
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
