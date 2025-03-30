using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCard : BaseCardEffect, ICardEffect
{
    private readonly RemoveCardSO _data;
    private readonly IBlackjackService _blackjackService;
    
    public RemoveCard(RemoveCardSO data, IBlackjackService blackjackService)
    {
        _data = data;
        _blackjackService = blackjackService;
    }
    public void OnExecute(PlayerType playerType, string cardID)
    {
        if (playerType == PlayerType.Player1)
        {
            _blackjackService.OnRequestCardDeletion(_data.target == PlayerFilter.Yourself
                ? PlayerType.Player1
                : PlayerType.Player2);
        }
        else
        {
            _blackjackService.OnRequestCardDeletion(_data.target == PlayerFilter.Yourself
                ? PlayerType.Player2
                : PlayerType.Player1);
        }
    }
}
