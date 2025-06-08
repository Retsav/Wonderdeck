using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class TutorialBlackjackScoring : MonoBehaviour
{
    [SerializeField] private TutorialBlackjackLogic tutorialBlackjackLogic;
    
    
    [SerializeField] private TextMeshProUGUI playerOneScoreLabel;
    [SerializeField] private TextMeshProUGUI playerSecondScoreLabel;

    private ITutorialService _tutorialService;

    [Inject]
    private void ResolveDependencies(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    public void RefreshScoring()
    {
        var firstPlayerScore = 0f;
        var secondPlayerScore = 0f;
        
        for (var index = 0; index < tutorialBlackjackLogic.FirstPlayerDeck.Count; index++)
        {
            var card = tutorialBlackjackLogic.FirstPlayerDeck[index];
            foreach (var cardEffect in card.DrawCardEffects)
            {
                if (cardEffect is AddValueEffectSO effect) firstPlayerScore += effect.cardValue;
            }
        }
        
        int cardsToCount = tutorialBlackjackLogic.ArbiterDeck.Count - (_tutorialService.HasHiddenCard ? 1 : 0);
        for (var index = 0; index < cardsToCount; index++)
        {
            var card = tutorialBlackjackLogic.ArbiterDeck[index];
            foreach (var cardEffect in card.DrawCardEffects)
            {
                if (cardEffect is AddValueEffectSO effect) secondPlayerScore += effect.cardValue;
            }
        }
        
        playerOneScoreLabel.SetText($"{firstPlayerScore}/{tutorialBlackjackLogic.Threshold}");
        if (_tutorialService.HasHiddenCard)
        {
            playerSecondScoreLabel.SetText(
                $"{secondPlayerScore}+?/{tutorialBlackjackLogic.Threshold}");
        }
        else
        {
            playerSecondScoreLabel.SetText(
                $"{secondPlayerScore}/{tutorialBlackjackLogic.Threshold}");
        }
    }
}
