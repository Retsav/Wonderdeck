using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New AddValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Debug Effect")]
public class DebugEffect : ScriptableObject, ICardEffect
{
    public void OnExecute()
    {
        Debug.Log("Execute");
    }
}
