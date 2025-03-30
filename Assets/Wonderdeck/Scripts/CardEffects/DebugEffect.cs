using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New AddValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Debug Effect")]
public class DebugEffect : BaseCardEffect, ICardEffect
{
    public void OnExecute(PlayerType playerType, string cardID)
    {
        Debug.Log("Execute");
    }
}
