using System;
using UnityEngine;

/// <summary>
/// Only place that knows what mask is active, everyone else queries, nobody sets.
/// </summary>

public class MaskManager : MonoBehaviour
{
    public event Action<MaskData> OnMaskEquipped;
    public event Action OnMaskBroken;
    public event Action OnPlayerDied;
    
    public MaskInstance CurrentMask { get; private set; }
    
    public void EquipMask(MaskData data)
    {
        if (CurrentMask != null)
            CurrentMask.OnBreak -= BreakCurrentMask;
    
        CurrentMask = new MaskInstance(data);
        CurrentMask.OnBreak += BreakCurrentMask;
        OnMaskEquipped?.Invoke(data);
    }

    public void BreakCurrentMask()
    {
        CurrentMask.OnBreak -= BreakCurrentMask;
        CurrentMask = null;
        OnMaskBroken?.Invoke();
    }

    public bool IsMaskless() => CurrentMask == null;

    public void ApplyDamage(int amount)
    {
        if (!IsMaskless())
            CurrentMask.TakeDamage(amount);
        else
            OnPlayerDied?.Invoke();
    }

    private void OnDestroy()
    {
        if (CurrentMask != null)
            CurrentMask.OnBreak -= BreakCurrentMask;
    }
}