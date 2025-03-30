using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New IncreaseDamageEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Increase Player Damage")]
public class IncreaseDamageEffectSO : CardEffectSO
{
    public int damageToIncrease;
    
    public override ICardEffect CreateEffect(DiContainer container)
    {
        return container.Instantiate<IncreaseDamageEffect>(new object[] { this });
    }
}
