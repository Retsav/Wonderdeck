using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;

public class BlackjackCardResolver : NetworkBehaviour
{
    private IBlackjackService _blackjackService;
    
    
    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }


    public override void OnStartClient()
    {
        if (!NetworkManager.ClientManager.Connection.IsHost) return;
        _blackjackService.CardPlayed += OnCardPlayed;
    }

    private void OnCardPlayed(object sender, CardPlayedEventArgs e)
    {
        CardSO card = _blackjackService.GetCardByID(e.CardID, NetworkManager.ClientManager.Connection, e.PlayerType);
        /*if (card == null)
        {
            Debug.LogError($"Could not find card with ID {e.CardID}");
            return;
        }*/

        switch (e.PlayType)
        {
            case PlayType.Draw:
                for (int i = 0; i < card.DrawCardEffects.Count; i++)
                {
                    ICardEffect effect = (ICardEffect)card.DrawCardEffects[i];
                    if (effect == null)
                    {
                        Debug.LogError($"Cast operation invalid. Does card effects is deriving from ICardEffect?");
                        return;
                    }
                    if(effect is AddValueEffect addValueEffect)
                    {
                        if (e.PlayerType == PlayerType.Player1)
                        {
                            _blackjackService.FirstPlayerScore += (int)addValueEffect.cardValue;
                            _blackjackService.OnScoreUpdated(new PlayerScoreUpdatedEventArgs(e.PlayerType, _blackjackService.FirstPlayerScore));
                        }
                        else
                        {
                            _blackjackService.SecondPlayerScore += (int)addValueEffect.cardValue;
                            _blackjackService.OnScoreUpdated(new PlayerScoreUpdatedEventArgs(e.PlayerType, _blackjackService.SecondPlayerScore));
                        }
                    }
                    effect.OnExecute(e.PlayerType);
                }
                break;
            case PlayType.Play:
                for (int i = 0; i < card.PlayCardEffects.Count; i++)
                {
                    ICardEffect effect = (ICardEffect)card.PlayCardEffects[i];
                    if (effect == null)
                    {
                        Debug.LogError($"Cast operation invalid. Does card effects is deriving from ICardEffect?");
                        return;
                    }
                    effect.OnExecute(e.PlayerType);
                }
                break;
            case PlayType.Discard:
                for (int i = 0; i < card.DiscardCardEffects.Count; i++)
                {
                    ICardEffect effect = (ICardEffect)card.DiscardCardEffects[i];
                    if (effect == null)
                    {
                        Debug.LogError($"Cast operation invalid. Does card effects is deriving from ICardEffect?");
                        return;
                    }
                    effect.OnExecute(e.PlayerType);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnDestroy()
    {
        _blackjackService.CardPlayed -= OnCardPlayed;
    }
}
