using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only place that knows what mask is active, everyone else queries, nobody sets.
/// </summary>

public class MaskManager : MonoBehaviour
{
    private const int MAX_MASK_STACK_SIZE = 3;

    public event Action<MaskData> OnMaskEquipped;
    public event Action OnMaskBroken;
    public event Action OnPlayerDied;

    public MaskInstance CurrentMask =>
        _maskStack.Count > 0 ? _maskStack.Peek() : null;

    private Stack<MaskInstance> _maskStack = new();

    public bool AddMaskToStack(MaskData maskData)
    {
        if (_maskStack.Count >= MAX_MASK_STACK_SIZE)
            return false;

        if (CurrentMask != null)
            CurrentMask.OnBreak -= BreakCurrentMask;

        var maskInstance = new MaskInstance(maskData);

        _maskStack.Push(maskInstance);

        CurrentMask.OnBreak += BreakCurrentMask;

        OnMaskEquipped?.Invoke(maskData);

        return true;
    }

    public void BreakCurrentMask()
    {
        if (CurrentMask == null)
            return;
        
        CurrentMask.OnBreak -= BreakCurrentMask;
        _maskStack.Pop();
        
        OnMaskBroken?.Invoke();

        if (CurrentMask == null) return;
        
        CurrentMask.OnBreak += BreakCurrentMask;
        OnMaskEquipped?.Invoke(CurrentMask.Data);
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