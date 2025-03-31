using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapCardsEffect : BaseCardEffect, ICardEffect
{
    private readonly SwapCardsEffectSO _data;
    private readonly IBlackjackService _blackjackService;
    
    private SwapCardsEffect(SwapCardsEffectSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    public void OnExecute(PlayerType playerType, string cardID) => _blackjackService.OnRequestCardSwap();
}
