using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardEffect
{
    void OnDraw();
    void OnPlay();
    void OnDiscard();
}
