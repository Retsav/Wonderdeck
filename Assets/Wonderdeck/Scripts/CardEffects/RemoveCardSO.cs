using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

[CreateAssetMenu(fileName = "New RemoveCard", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Remove Card")]
public class RemoveCardSO : CardEffectSO
{
    public PlayerFilter target;
    
    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<RemoveCard>(new object[] { this });
}
