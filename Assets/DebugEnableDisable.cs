using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEnableDisable : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.LogError($"{gameObject.name} enabled");
    }

    private void OnDisable()
    {
        Debug.LogError($"{gameObject.name} disabled");
    }
}
