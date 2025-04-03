using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerTypeExtensions
{
    public static PlayerType GetOppositeType(this PlayerType playerType)
    {
        return playerType == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
    }
}

