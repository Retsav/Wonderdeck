using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICardEffect
{
    
    public void OnExecute(PlayerType playerType, string cardID);
}
