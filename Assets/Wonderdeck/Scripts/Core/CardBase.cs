using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CardBase
{
    public CardSO cardSO;

    public virtual void OnDraw()
    {
        for (int i = 0; i < cardSO.DrawCardEffects.Count; i++) cardSO.DrawCardEffects[i].OnDraw();
    }

    public virtual void OnPlay()
    {
        for (int i = 0; i < cardSO.DrawCardEffects.Count; i++) cardSO.DrawCardEffects[i].OnPlay();
    }

    public virtual void OnDiscard()
    {
        for (int i = 0; i < cardSO.DrawCardEffects.Count; i++) cardSO.DrawCardEffects[i].OnDiscard();
    }
}
