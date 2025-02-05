using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "[CARD EFFECT] SwapCardsEffectSO", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Swap Cards Effect")]
public class SwapCardsEffectSO : CardEffectSO
{
    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<SwapCardsEffect>(new object[] {this});
}
