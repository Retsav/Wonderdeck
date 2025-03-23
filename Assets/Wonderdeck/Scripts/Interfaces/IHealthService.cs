using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealthService
{
    public int MaxHealth { get; set; }
    public int FirstPlayerHealth { get; set; }
    public int SecondPlayerHealth { get; set; }

    public void ApplyDamage(RoundResult result);
    public void ApplyDamage(int damage, PlayerType playerType);
    public void OnDamageApplied(int damageP1, int damageP2);

    public event EventHandler<DamageAppliedEventArgs> DamageAppliedEvent;
    public event EventHandler<RoundResult> ApplyDamageViaResultEvent;
    public event EventHandler<ApplyDamageEventArgs> ApplyDamageViaNumberEvent;
}

public class DamageAppliedEventArgs : EventArgs
{
    public int DamageP1;
    public int DamageP2;

    public DamageAppliedEventArgs(int damageP1, int damageP2)
    {
        DamageP1 = damageP1;
        DamageP2 = damageP2;
    }
}

public class ApplyDamageEventArgs: EventArgs
{
    public PlayerType PlayerType;
    public int DamageNumber;

    public ApplyDamageEventArgs(PlayerType playerType, int damageNumber)
    {
        PlayerType = playerType;
        DamageNumber = damageNumber;
    }
}

