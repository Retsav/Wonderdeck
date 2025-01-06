using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New RemoveValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Remove Value")]
public class RemoveValueEffect : ScriptableObject, ICardEffect
{
    public float valueToRemove = 1;
    public void OnExecute(PlayerType playerType)
    {
        
    }
}
