using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScoreLimitEffect : BaseCardEffect, ICardEffect
{
    private readonly SetScoreLimitEffectSO _data;
    private readonly IBlackjackService _blackjackService;


    public SetScoreLimitEffect(SetScoreLimitEffectSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    
    public void OnExecute(PlayerType playerType, string cardID)
    {
        _blackjackService.OnGameScoreUpdated(_data.newScore);
    }
}
