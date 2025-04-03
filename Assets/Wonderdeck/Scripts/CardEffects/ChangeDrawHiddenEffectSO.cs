using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "[CARD EFFECT] ChangeDrawHiddenEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] ChangeDrawHiddenEffect")]
public class ChangeDrawHiddenEffectSO : CardEffectSO
{
    public bool drawHidden;
    public PlayerFilter playerFilter;
    
    
    public override ICardEffect CreateEffect(DiContainer container)
    {
        return container.Instantiate<ChangeDrawHiddenEffect>(new object[] { this });
    }
}
