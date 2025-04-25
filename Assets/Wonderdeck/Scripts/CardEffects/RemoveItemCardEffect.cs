using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveItemCardEffect : BaseCardEffect, ICardEffect
{
    private readonly RemoveItemCardEffectSO _data;
    private readonly ISelectModeService _selectModeService;
    private readonly IBlackjackService _blackjackService;
    private readonly ITutorialService _tutorialService;
    
    public RemoveItemCardEffect(RemoveItemCardEffectSO data, ISelectModeService selectModeService, IBlackjackService blackjackService, ITutorialService tutorialService)
    {
        _data = data;
        _selectModeService = selectModeService;
        _blackjackService = blackjackService;
        _tutorialService = tutorialService;
    }
    
    
    public void OnExecute(PlayerType playerType, string cardID)
    {
        if(_tutorialService.IsTutorial)
            _selectModeService.EnterSelectionMode(cardID);
        else
            _selectModeService.OnRequestConnectionModeEnter(cardID, playerType);
    }

    public override void OnSelectionModeExecute(string selectedCardID, PlayerType cardOwner)
    {
        _blackjackService.OnCardPlayed(new CardPlayedEventArgs(selectedCardID, cardOwner, PlayType.Discard));
    }
}
