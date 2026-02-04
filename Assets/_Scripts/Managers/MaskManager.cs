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

    [Header("MASKLESS STATE")] [SerializeField]
    private MaskData _masklessData;
    
    [Header("INVINCIBILITY")]
    [SerializeField] private PlayerBaseStats _playerStats;
    [SerializeField] private SpriteRenderer _playerSprite;

    [Header("AUDIO CLIPS")] 
        [SerializeField] GlobalSoundsData _globalSounds;

    public event Action<MaskData> OnMaskEquipped;
    public event Action OnMaskBroken;
    public event Action OnPlayerDied;
    public event Action<int> OnDamageReceived;

    public bool IsInvincible => _isInvincible;
    public int CurrentHP => IsMaskless() ? 1 : CurrentMask.HP;
    public MaskInstance CurrentMask =>
        _maskStack.Count > 0 ? _maskStack.Peek() : _masklessInstance;

    private Stack<MaskInstance> _maskStack = new();
    private bool _isInvincible;
    private Coroutine _invincibilityRoutine;
    private MaskInstance _masklessInstance;

    private void Start()
    {
        _masklessInstance = new MaskInstance(_masklessData);
        SoundFXManager.Instance.PlayLoop(_globalSounds.Playing, transform);
    }

    public bool AddMaskToStack(MaskData maskData)
    {
        if (_maskStack.Count >= MAX_MASK_STACK_SIZE)
            return false;
        
        SoundFXManager.Instance.Play(_globalSounds.PlayerEquipMask, _playerSprite.transform);

        if (_maskStack.Count > 0)
            CurrentMask.OnBreak -= BreakCurrentMask;

        var maskInstance = new MaskInstance(maskData);

        _maskStack.Push(maskInstance);

        CurrentMask.OnBreak += BreakCurrentMask;

        PauseManager.Instance.FreezePlayerFor(1.05f);
        OnMaskEquipped?.Invoke(maskData);
        
        return true;
    }

    public void BreakCurrentMask()
    {
        if (_maskStack.Count == 0)
            return;
        
        SoundFXManager.Instance.Play(_globalSounds.PlayerBreakMask, _playerSprite.transform);
        
        CurrentMask.OnBreak -= BreakCurrentMask;
        _maskStack.Pop();
        
        OnMaskBroken?.Invoke();

        if (_maskStack.Count == 0)
            return;

        CurrentMask.OnBreak += BreakCurrentMask;
        PauseManager.Instance.FreezePlayerFor(1.05f);
        OnMaskEquipped?.Invoke(CurrentMask.Data);
    }

    public bool IsMaskless() => CurrentMask == _masklessInstance;

    #region Attack and secondary type getters

    public AttackType GetCurrentAttack() => CurrentMask.Data.AttackType;

    public SecondaryType GetCurrentSecondary() => CurrentMask.Data.SecondaryType;

    public bool HasDoubleJump() => CurrentMask.Data.DoubleJump;

    public bool HasDash() => CurrentMask.Data.SecondaryType == SecondaryType.Dash;

    public bool HasTeleport() => CurrentMask.Data.SecondaryType == SecondaryType.Teleport;
    
    public RangedAttackData GetCurrentRangedAttack() =>
        CurrentMask.Data.AttackType != AttackType.Ranged 
            ? null 
            : CurrentMask.Data.RangedProjectile;
    
    #endregion
    
    public void ApplyDamage(int amount)
    {
        if (_isInvincible)
            return;
        
        SoundFXManager.Instance.Play(_globalSounds.PlayerHit, _playerSprite.transform);

        OnDamageReceived?.Invoke(amount);
        if (!IsMaskless())
            CurrentMask.TakeDamage(amount);
        else
        {
            Cursor.visible = true;
            SoundFXManager.Instance.StopLoop(transform);
            SoundFXManager.Instance.Play(_globalSounds.GameOver, _playerSprite.transform);
            PauseManager.Instance.FreezePlayer();
            PauseManager.Instance.SetTimeFreeze(0);
            _playerSprite.transform.parent.parent.gameObject.SetActive(false);
            OnPlayerDied?.Invoke();
        }

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