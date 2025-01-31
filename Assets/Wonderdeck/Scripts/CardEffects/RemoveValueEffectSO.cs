using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New RemoveValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Remove Value")]
public class RemoveValueEffectSO : CardEffectSO
{
    public float valueToRemove = 1;
    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<RemoveValueEffect>(new object[] { this });
}
