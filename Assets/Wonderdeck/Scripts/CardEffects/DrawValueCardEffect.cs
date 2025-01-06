using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Zenject;


[CreateAssetMenu(fileName = "New DrawValueCardEffect", menuName = "Wonderdeck/Card Effects/[CARD EFFECT] Draw Value Card Effect")]
public class DrawValueCardEffect : ScriptableObject, ICardEffect
{
    public float valueToDraw;
    private IBlackjackService _blackjackService;
    private PlayerType _playerType;


    [Inject]
    private void ResolveDependencies(IBlackjackService blackjackService)
    {
        _blackjackService = blackjackService;
    }
    
    public void OnExecute(PlayerType playerType)
    {
        /*_playerType = playerType;
        GetCardWithValueServerRpc(); */
    }
    
    private void GetCardWithValueServerRpc() => _blackjackService.OnGetCardWithSpecificValue(new GetCardWithSpecificValueEventArgs(valueToDraw, _playerType));
}
