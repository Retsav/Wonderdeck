using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialDamageCalculatorUI : MonoBehaviour
{
    [SerializeField] private TutorialBlackjackLogic tutorialBlackjackLogic;
    [SerializeField] private TextMeshProUGUI _firstPlayerDamageLabel;
    [SerializeField] private TextMeshProUGUI _secondPlayerDamageLabel;


    private int secondPlayerModifier = 0;
    private Color _orginalTextColor;

    private void Start()
    {
        _orginalTextColor = _secondPlayerDamageLabel.color;
    }

    public void ChangeModifier(int modifier)
    {
        secondPlayerModifier = modifier;
    }
    
    
    public void RefreshDamageCalculation()
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
        
        for (var index = 0; index < tutorialBlackjackLogic.ArbiterDeck.Count; index++)
        {
            var card = tutorialBlackjackLogic.ArbiterDeck[index];
            foreach (var cardEffect in card.DrawCardEffects)
            {
                if (cardEffect is AddValueEffectSO effect) secondPlayerScore += effect.cardValue;
            }
        }
        
        



        var firstPlayerDamage = (int)Math.Abs(firstPlayerScore - tutorialBlackjackLogic.Threshold);
        var secondPlayerDamage = (int)Math.Abs(secondPlayerScore - tutorialBlackjackLogic.Threshold);
        
        
        if (secondPlayerModifier > 0)
        {
            secondPlayerDamage += secondPlayerModifier;
            _secondPlayerDamageLabel.DOColor(Color.red, 0.3f);
        } else if (secondPlayerModifier < 0)
        {
            secondPlayerDamage += secondPlayerModifier;
            secondPlayerDamage = Math.Max(secondPlayerDamage, 0);
            _secondPlayerDamageLabel.DOColor(Color.green, 0.3f);
        }
        else
            _secondPlayerDamageLabel.DOColor(_orginalTextColor, 0.3f);
        
        _firstPlayerDamageLabel.SetText(firstPlayerDamage.ToString());
        _secondPlayerDamageLabel.SetText(secondPlayerDamage.ToString());
    }
}
