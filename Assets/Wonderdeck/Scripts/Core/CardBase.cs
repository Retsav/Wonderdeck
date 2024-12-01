using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CardBase
{
    public CardSO cardSO;

    public virtual void OnDraw()
    {
        for (int i = 0; i < cardSO.DrawCardEffects.Count; i++)
            if (cardSO.DrawCardEffects[i] is ICardEffect cardEffect) cardEffect.OnExecute();
    }

    public virtual void OnPlay()
    {
        for (int i = 0; i < cardSO.PlayCardEffects.Count; i++)
            if(cardSO.PlayCardEffects[i] is ICardEffect cardEffect) cardEffect.OnExecute();
    }

    public virtual void OnDiscard()
    {
        for (int i = 0; i < cardSO.DiscardCardEffects.Count; i++) 
            if(cardSO.DiscardCardEffects[i] is ICardEffect cardEffect) cardEffect.OnExecute();
    }
}
