using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New AddValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Debug Effect")]
public class DebugEffect : CardEffectSO, ICardEffect
{
    public void OnExecute(PlayerType playerType)
    {
        Debug.Log("Execute");
    }

    public override ICardEffect CreateEffect(DiContainer container)
    {
        throw new System.NotImplementedException();
    }
}
