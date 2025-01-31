using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "[CARD EFFECT] SetScoreLimitEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Set Score Limit Effect")]
public class SetScoreLimitEffectSO : CardEffectSO
{
    public int newScore;
    
    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<SetScoreLimitEffect>(new object[] {this});
}
