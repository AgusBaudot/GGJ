using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Only place that knows what mask is active, everyone else queries, nobody sets.
/// Handles damage application and invincibility (blink) after hit.
/// </summary>

public class MaskManager : MonoBehaviour
{
    private const int MAX_MASK_STACK_SIZE = 3;

    [Header("INVINCIBILITY")]
    [SerializeField] private PlayerBaseStats _playerStats;
    [SerializeField] private SpriteRenderer _playerSprite;

    public event Action<MaskData> OnMaskEquipped;
    public event Action OnMaskBroken;
    public event Action OnPlayerDied;
    public event Action<int> OnDamageReceived;

    public bool IsInvincible => _isInvincible;
    public MaskInstance CurrentMask =>
        _maskStack.Count > 0 ? _maskStack.Peek() : null;

    private Stack<MaskInstance> _maskStack = new();
    private bool _isInvincible;
    private Coroutine _invincibilityRoutine;

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

    #region Attack and secondary type getters
    //What happens when player wants to attack and/or presses secondary button while maskless?
    public AttackType GetCurrentAttack()
    {
        return CurrentMask != null
            ? CurrentMask.Data.AttackType
            : AttackType.Basic;
    }

    public SecondaryType GetCurrentSecondary()
    {
        return CurrentMask != null
            ? CurrentMask.Data.SecondaryType
            : SecondaryType.None;
    }

    public bool HasDoubleJump()
    {
        return CurrentMask != null
            ? CurrentMask.Data.DoubleJump
            : false;
    }

    public bool HasDash()
    {
        return CurrentMask != null
            ? GetCurrentSecondary() == SecondaryType.Dash
            : false;
    }

    public bool HasTeleport()
    {
        return CurrentMask != null
            ? GetCurrentSecondary() == SecondaryType.Teleport
            : false;
    }
    #endregion

    public RangedAttackData GetCurrentRangedAttack()
    {
        if (IsMaskless()) return null;

        if (CurrentMask.Data.AttackType != AttackType.Ranged) return null;
        
        return CurrentMask.Data.RangedProjectile;
    }

    public void ApplyDamage(int amount)
    {
        if (_isInvincible)
            return;

        OnDamageReceived?.Invoke(amount);
        if (!IsMaskless())
            CurrentMask.TakeDamage(amount);
        else
            OnPlayerDied?.Invoke();

        StartInvincibility();
    }

    private void StartInvincibility()
    {
        if (_invincibilityRoutine != null)
            StopCoroutine(_invincibilityRoutine);
        _invincibilityRoutine = StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        _isInvincible = true;

        float duration = _playerStats != null ? _playerStats.InvincibilityDuration : 1.5f;
        float interval = _playerStats != null ? _playerStats.BlinkInterval : 0.1f;
        float flashAlpha = _playerStats != null ? _playerStats.FlashAlpha : 0.3f;

        bool visible = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (_playerSprite != null)
            {
                Color c = _playerSprite.color;
                _playerSprite.color = new Color(c.r, c.g, c.b, visible ? 1f : flashAlpha);
                visible = !visible;
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if (_playerSprite != null)
        {
            Color c = _playerSprite.color;
            _playerSprite.color = new Color(c.r, c.g, c.b, 1f);
        }

        _isInvincible = false;
        _invincibilityRoutine = null;
    }

    private void OnDestroy()
    {
        if (_invincibilityRoutine != null)
            StopCoroutine(_invincibilityRoutine);
        if (CurrentMask != null)
            CurrentMask.OnBreak -= BreakCurrentMask;
    }
}