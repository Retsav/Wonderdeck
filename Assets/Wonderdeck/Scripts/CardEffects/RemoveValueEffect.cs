using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RemoveValueEffect : ICardEffect
{
    private readonly RemoveValueEffectSO _data;
    private readonly IBlackjackService _blackjackService;

    public RemoveValueEffect(RemoveValueEffectSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    
    public void OnExecute(PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
            _blackjackService.FirstPlayerScore -= (int)_data.valueToRemove;
        else
            _blackjackService.SecondPlayerScore -= (int)_data.valueToRemove;
    }
}
