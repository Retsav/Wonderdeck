using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class StartTutorialUI : MonoBehaviour
{
    [SerializeField] private WonderdeckButton startTutorialButton;


    private void Start()
    {
        startTutorialButton.onClick.RemoveAllListeners();
        startTutorialButton.onClick.AddListener(GoToTutorial);
    }

    private void GoToTutorial()
    {
        SceneManager.LoadScene("DebugScene 2 TUTORIAL");
        SceneManager.LoadScene("DebugUISCene Tutorial", LoadSceneMode.Additive);
    }

    private void OnDestroy()
    {
        startTutorialButton.onClick.RemoveAllListeners();
    }
}
