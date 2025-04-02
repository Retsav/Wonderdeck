using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New DrawValueCardEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Draw Value Card Effect")]
public class DrawValueCardEffectSO : CardEffectSO
{
    public float valueToDraw;
    
    public override ICardEffect CreateEffect(DiContainer container)
    {
        return container.Instantiate<DrawValueCardEffect>(new object[] { this });
    }
}
