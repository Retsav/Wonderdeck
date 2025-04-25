using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialPlayerHealthVisuals : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI firstPlayerHealthLabel;
    [SerializeField] private TextMeshProUGUI secondPlayerHealthLabel;
    [SerializeField] private GameObject firstPlayerHealthBar;
    [SerializeField] private GameObject secondPlayerHealthBar;



    public void SetHealth(int health, PlayerType playerType)
    {
        var textToTarget = playerType == PlayerType.Player1 ? firstPlayerHealthLabel : secondPlayerHealthLabel;
        textToTarget.SetText(health.ToString());
    }
}

