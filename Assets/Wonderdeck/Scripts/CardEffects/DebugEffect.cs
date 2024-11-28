using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEffect : ICardEffect
{
    public void OnDraw()
    {
        Debug.Log("DebugEffect OnDraw");
    }

    public void OnPlay()
    {
        Debug.Log("DebugEffect OnPlay");
    }

    public void OnDiscard()
    {
        Debug.Log("DebugEffect OnDiscard");
    }
}
