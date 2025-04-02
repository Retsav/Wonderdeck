using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "[CARD EFFECT] New RemoveItemCardEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Remove Item Card Effect")]
public class RemoveItemCardEffectSO : CardEffectSO
{
    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<RemoveItemCardEffect>(new object[] { this });
    
}
