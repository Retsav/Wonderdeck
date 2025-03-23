using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthService : IHealthService
{
    public int MaxHealth { get; set; } = 100;
    public int FirstPlayerHealth { get; set; } = 100;
    public int SecondPlayerHealth { get; set; } = 100;
    
    
    public void ApplyDamage(RoundResult result) => ApplyDamageViaResultEvent?.Invoke(this, result);
    public void ApplyDamage(int damage, PlayerType playerType) => ApplyDamageViaNumberEvent?.Invoke(this, new ApplyDamageEventArgs(playerType, damage));
    public void OnDamageApplied(int damageP1, int damageP2) => DamageAppliedEvent?.Invoke(this, new DamageAppliedEventArgs(damageP1, damageP2));

    public event EventHandler<DamageAppliedEventArgs> DamageAppliedEvent;
    public event EventHandler<RoundResult> ApplyDamageViaResultEvent;
    public event EventHandler<ApplyDamageEventArgs> ApplyDamageViaNumberEvent;
}
