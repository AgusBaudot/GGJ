using System;
using UnityEngine;

/// <summary>
/// Calculate the stats at runtime.
/// </summary>

[Serializable]
public class PlayerStatsRuntime
{
    //Final stats = base stats x mask mods

    private readonly PlayerBaseStats _baseStats;
    private readonly MaskManager _maskManager;

    public PlayerStatsRuntime(PlayerBaseStats baseStats, MaskManager maskManager)
    {
        _baseStats = baseStats;
        _maskManager = maskManager;
    }

    public float GetFinalSpeed()
    {
        // float finalSpeed = _baseStats.BaseMoveSpeed;
        // if (_maskManager.IsMaskless())
        //     return finalSpeed;

        // return finalSpeed * _maskManager.CurrentMask.Data.SpeedModifier;
        Debug.LogWarning("Final speed is now legacy. Use PlayerBaseStats instead.");
        return 0;
    }

    public float GetFinalJumpForce()
    {
        // float finalJumpForce = _baseStats.BaseJumpForce;
        // if (_maskManager.IsMaskless())
        //     return finalJumpForce;
        //
        // return finalJumpForce * _maskManager.CurrentMask.Data.JumpForceModifier;
        Debug.LogWarning("FinalJumpForce is now legacy. Use PlayerBaseStats instead.");
        return 0;
    }

    public float GetFinalDamage()
    {
        float finalDmg = _baseStats.BaseDamage;
        if (_maskManager.IsMaskless())
            return finalDmg;

        return finalDmg * _maskManager.CurrentMask.Data.DmgModifier;
    }
}