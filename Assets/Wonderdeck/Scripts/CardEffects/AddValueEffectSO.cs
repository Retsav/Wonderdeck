using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New AddValueEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Add Value")]
public class AddValueEffectSO : CardEffectSO
{
    public float cardValue = 1;

    public override ICardEffect CreateEffect(DiContainer container) => container.Instantiate<AddValueEffect>(new object[] { this });
}
