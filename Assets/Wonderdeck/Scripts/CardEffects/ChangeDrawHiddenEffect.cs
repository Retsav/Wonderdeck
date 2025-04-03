using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDrawHiddenEffect : BaseCardEffect, ICardEffect
{
    private readonly ChangeDrawHiddenEffectSO _data;
    private readonly IBlackjackService _blackjackService;
    
    public ChangeDrawHiddenEffect(ChangeDrawHiddenEffectSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    
    
    public void OnExecute(PlayerType playerType, string cardID)
    {
        _blackjackService.ChangeDrawHidden(playerType, _data.playerFilter, _data.drawHidden);
    }
}
