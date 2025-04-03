using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealCards : BaseCardEffect, ICardEffect
{
    private readonly RevealCardsSO _data;
    private readonly IBlackjackService _blackjackService;
    
    public RevealCards(RevealCardsSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }

    public void OnExecute(PlayerType playerType, string cardID)
    {
        _blackjackService.OnRevealCards(playerType, _data.target);
    }
}
