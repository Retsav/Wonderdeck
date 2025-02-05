using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FishNet.Object;
using TMPro;
using UnityEngine;
using Zenject;

public class RoundResultUI : NetworkBehaviour
{

    [SerializeField] private TextMeshProUGUI roundResultLabel;

    private Sequence textSequence;
    
    private IBlackjackService _blackjackService;

    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }

    public override void OnStartClient()
    {
        _blackjackService.RoundConsequencesEvaluated += OnBlackjackRoundEvaluated;
        roundResultLabel.DOFade(0f, 0f);
    }

    private void OnBlackjackRoundEvaluated(object sender, RoundConsequencesEvaluatedEventArgs e)
    {
        switch (e.Result)
        {
            case RoundResult.Draw:
                roundResultLabel.text =
                    $"Draw!\n{e.FinalFirstPlayerScore} vs {e.FinalSecondPlayerScore}";
                break;
            case RoundResult.BothPlayersLost:
                roundResultLabel.text =
                    $"Both players lost!\n{e.FinalFirstPlayerScore} vs {e.FinalSecondPlayerScore}";
                break;
            case RoundResult.FirstPlayerLost:
                roundResultLabel.text = NetworkManager.ClientManager.Connection.IsHost ? 
                    $"You lost this round!\n{e.FinalFirstPlayerScore} vs {e.FinalSecondPlayerScore}" : 
                    $"You won this round!\n{e.FinalFirstPlayerScore} vs {e.FinalSecondPlayerScore}";
                break;
            case RoundResult.SecondPlayerLost:
                roundResultLabel.text = !NetworkManager.ClientManager.Connection.IsHost ? 
                    $"You lost this round!\n{e.FinalFirstPlayerScore} vs {e.FinalSecondPlayerScore}" : 
                    $"You won this round!\n{e.FinalFirstPlayerScore} vs {e.FinalSecondPlayerScore}";
                break;
        }

        textSequence = DOTween.Sequence();
        textSequence.Append(roundResultLabel.DOFade(1f, 2f));
        textSequence.Append(roundResultLabel.DOFade(0f, 2f));
    }

    private void OnDestroy()
    {
        _blackjackService.RoundConsequencesEvaluated -= OnBlackjackRoundEvaluated;
    }
}
