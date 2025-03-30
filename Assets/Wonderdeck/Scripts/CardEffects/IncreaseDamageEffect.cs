using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDamageEffect : BaseCardEffect, ICardEffect
{
    private readonly IncreaseDamageEffectSO _data;
    private readonly IHealthService _healthService;

    public IncreaseDamageEffect(IncreaseDamageEffectSO data, IHealthService healthService)
    {
        _data = data;
        _healthService = healthService;
    }
    public void OnExecute(PlayerType playerType, string cardID) => _healthService.OnChangedDamageModifier(_data.damageToIncrease, _data.target, playerType);
}
