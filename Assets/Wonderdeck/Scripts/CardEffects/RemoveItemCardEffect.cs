using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveItemCardEffect : BaseCardEffect, ICardEffect
{
    private readonly RemoveItemCardEffectSO _data;
    private readonly ISelectModeService _selectModeService;
    private readonly IBlackjackService _blackjackService;
    
    public RemoveItemCardEffect(RemoveItemCardEffectSO data, ISelectModeService selectModeService, IBlackjackService blackjackService)
    {
        _data = data;
        _selectModeService = selectModeService;
        _blackjackService = blackjackService;
    }
    
    
    public void OnExecute(PlayerType playerType, string cardID)
    {
        _selectModeService.OnRequestConnectionModeEnter(cardID, playerType);
    }

    public override void OnSelectionModeExecute(string selectedCardID, PlayerType cardOwner)
    {
        _blackjackService.OnCardPlayed(new CardPlayedEventArgs(selectedCardID, cardOwner, PlayType.Discard));
    }
}
