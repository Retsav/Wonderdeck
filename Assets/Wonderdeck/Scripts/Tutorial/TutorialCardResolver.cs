using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TutorialCardResolver : MonoBehaviour
{
    [SerializeField] private TutorialCardVisuals tutorialCardVisuals;
    [SerializeField] private TutorialBlackjackScoring tutorialBlackjackScoring;
    [SerializeField] private TutorialDamageCalculatorUI tutorialDamageCalculatorUI;
    
    
    
    
    
    private DiContainer _container;

    private ITutorialService _tutorialService;
    
    [Inject]
    private void ResolveDependencies(DiContainer container, ITutorialService tutorialService)
    {
        _container = container;
        _tutorialService = tutorialService;
    }

    private void Start()
    {
        _tutorialService.ExecutedItem += OnExecutedItem;
    }

    private void OnDestroy()
    {
        _tutorialService.ExecutedItem -= OnExecutedItem;
    }

    private void OnExecutedItem(object sender, CardSO e)
    {
        ResolveCard(e, PlayerType.Player1, PlayType.Play);
    }

    public void ResolveCard(CardSO card, PlayerType playerType, PlayType playType)
    {
        switch (playType)
        {
            case PlayType.Draw:
                ResolveEffects(card.DrawCardEffects, playerType, card.CardId);
                break;
            case PlayType.Play:
                ResolveEffects(card.PlayCardEffects, playerType, card.CardId);
                break;
            case PlayType.Discard:
                ResolveEffects(card.DiscardCardEffects, playerType, card.CardId);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if(playType == PlayType.Draw)
            tutorialCardVisuals.SpawnCardVisual(card, playerType, false);
        tutorialBlackjackScoring.RefreshScoring();
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }

    public void SpawnCardHidden(CardSO card)
    {
        tutorialCardVisuals.SpawnCardVisual(card, PlayerType.Player2, true);
        tutorialBlackjackScoring.RefreshScoring();
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }
    
    public void RevealHiddenCards()
    {
        tutorialCardVisuals.RevealHiddenCards();
        tutorialBlackjackScoring.RefreshScoring();
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }
    
    public void DestroyAllCards()
    {
        tutorialCardVisuals.DestroyAllCards();
        tutorialBlackjackScoring.RefreshScoring();
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }

    public void DestroyCard(string card, PlayerType playerType)
    {
        tutorialCardVisuals.DestroyCard(card, playerType);
        tutorialBlackjackScoring.RefreshScoring();
        tutorialDamageCalculatorUI.RefreshDamageCalculation();
    }

    private void ResolveEffects(List<CardEffectSO> effects, PlayerType playerType, string cardID)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            var effect = effects[i].CreateEffect(_container);
            effect.OnExecute(playerType, cardID);
        }
    }


}


