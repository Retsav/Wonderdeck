using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "[CARD EFFECT] New RevealCardsEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Reveal Cards Effect")]
public class RevealCardsSO : CardEffectSO
{
    public PlayerFilter target;
    
    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<RevealCards>(new object[] { this });
}
