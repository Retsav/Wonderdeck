using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    
    private void Start() => SceneManager.LoadScene(sceneToLoad);
}
