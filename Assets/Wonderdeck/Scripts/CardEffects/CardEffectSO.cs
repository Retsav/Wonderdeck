using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public abstract class CardEffectSO : ScriptableObject
{
    public abstract ICardEffect CreateEffect(DiContainer container);
}
