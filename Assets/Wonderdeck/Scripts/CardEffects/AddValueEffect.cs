using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New AddValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Add Value")]
public class AddValueEffect : ScriptableObject, ICardEffect
{
    [SerializeField] private float cardValue = 1;
    
    public void OnExecute()
    {
        Debug.Log($"Added value: {cardValue}");
    }
}
