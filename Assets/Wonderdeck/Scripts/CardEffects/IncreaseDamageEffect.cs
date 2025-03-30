using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDamageEffect : ICardEffect
{
    private readonly IncreaseDamageEffectSO _data;
    private readonly IHealthService _healthService;

    public IncreaseDamageEffect(IncreaseDamageEffectSO data, IHealthService healthService)
    {
        _data = data;
        _healthService = healthService;
    }
    public void OnExecute(PlayerType playerType) => _healthService.OnChangedDamageModifier(_data.damageToIncrease, playerType);
}
