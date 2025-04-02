using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCardEffect
{
    public virtual void OnSelectionModeExecute(string cardID, PlayerType cardOwner) {}
}
