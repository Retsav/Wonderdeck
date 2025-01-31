using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddValueEffect : ICardEffect
{
    private readonly AddValueEffectSO _data;
    private readonly IBlackjackService _blackjackService;

    public AddValueEffect(AddValueEffectSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    
    public void OnExecute(PlayerType playerType)
    {
        if (playerType == PlayerType.Player1)
            _blackjackService.FirstPlayerScore += (int)_data.cardValue;
        else
            _blackjackService.SecondPlayerScore += (int)_data.cardValue;
    }
}
