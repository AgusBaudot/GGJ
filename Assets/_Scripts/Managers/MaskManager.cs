using System;
using System.Reflection.Metadata;
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
    
    public Stack<MaskInstance> Masks;
    public MaskInstance CurrentMask { get; private set; }


    public bool AddMaskToStack(MaskData maskData)
    {

        if (Masks.Count >= MAX_MASK_STACK_SIZE)
            return false;

        if (CurrentMask != null)
            CurrentMask.OnBreak -= BreakCurrentMask;

        var maskInstance = new MaskInstance(maskData);

        Masks.Push(maskInstance);
        CurrentMask = maskInstance;

        CurrentMask.OnBreak += BreakCurrentMask;

        OnMaskEquipped?.Invoke(maskData);

        return true;

    }
    
    public void BreakCurrentMask()
    {
        if(CurrentMask == null) 
            return;
            
        CurrentMask.OnBreak -= BreakCurrentMask;

        Masks.Pop();
        CurrentMask = maskStack.Count > 0 
            ? maskStack.Peek() 
            : null;

        OnMaskBroken?Invoke();
    }

    public bool IsMaskless() => CurrentMask == null;

    public void ApplyDamage(int amount)
    {
        if (!IsMaskless())
            CurrentMask.TakeDamage(amount);
        else
            OnPlayerDied?.Invoke();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.LogWarning("Implement aim");
        }
        
        if (Input.GetKey(KeyCode.Q))
        {
            Debug.LogError("Not found");
        }
    }

    private void OnDestroy()
    {
        if (CurrentMask != null)
            CurrentMask.OnBreak -= BreakCurrentMask;
    }
}