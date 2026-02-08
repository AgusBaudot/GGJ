using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the enemy Animator based on state: idle (player out of range), run (moving), jump (in air), attack (trigger), die (trigger).
/// Guardian-specific: GrabWindup (trigger), GrabSuccess (trigger), GrabFailed (trigger) for windup → success/failed flow.
/// Stun handling: Freezes animation and applies visual tint when enemy is stunned.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Duration of death animation before the enemy is destroyed.")]
    [SerializeField] private float _deathAnimationDuration = 0.5f;
    [Tooltip("Horizontal speed threshold to consider the enemy 'running'.")]
    [SerializeField] private float _runSpeedThreshold = 0.1f;
    
    [Header("Stun Visual Settings")]
    [Tooltip("Color tint applied when enemy is stunned (default: light blue).")]
    [SerializeField] private Color _stunTintColor = new Color(0.7f, 0.7f, 1f);

    private Enemy _enemy;
    private Animator _anim;
    private SpriteRenderer _spriteRenderer;
    private bool _isGrounded;
    private bool _cachedQueryStartInColliders;

    public float DeathAnimationDuration => _deathAnimationDuration;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
        _anim = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Start() => _anim.SetFloat(EnemyIDKey, _enemy.Data.EnemyAnimationID);

    private void OnEnable()
    {
        if (_enemy != null)
        {
            _enemy.OnAttackTriggered += TriggerAttack;
            _enemy.OnAttackWindupStarted += TriggerGrabWindup;
            _enemy.OnAttackSucceeded += TriggerGrabSuccess;
            _enemy.OnAttackFailed += TriggerGrabFailed;
            _enemy.OnStunned += OnEnemyStunned;
            _enemy.OnStunRecovered += OnEnemyStunRecovered;
        }
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnAttackTriggered -= TriggerAttack;
            _enemy.OnAttackWindupStarted -= TriggerGrabWindup;
            _enemy.OnAttackSucceeded -= TriggerGrabSuccess;
            _enemy.OnAttackFailed -= TriggerGrabFailed;
            _enemy.OnStunned -= OnEnemyStunned;
            _enemy.OnStunRecovered -= OnEnemyStunRecovered;
        }
    }

    private void Update()
    {
        if (_enemy == null || !_enemy.IsAlive || _enemy.Data == null) return;

        CheckGrounded();

        // Handle stun state
        if (_enemy.IsStunned)
        {
            // Freeze animation
            _anim.speed = 0f;
            
            // Apply visual stun tint
            if (_spriteRenderer != null)
                _spriteRenderer.color = _stunTintColor;
            
            // Reset animation bools to prevent stuck states
            _anim.SetBool(IdleKey, false);
            _anim.SetBool(WalkingKey, false);
            _anim.SetBool(GrabWindupKey, false); // Cancel any ongoing windup
            
            // Set stunned parameter for animator (in case you want a dedicated stun state)
            _anim.SetBool(StunnedKey, true);
        }
        else
        {
            // Normal animation speed
            _anim.speed = 1f;
            
            // Reset visual tint
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.white;
            
            // Update movement animations
            bool isMoving = Mathf.Abs(_enemy.Rb.velocity.x) > _runSpeedThreshold;
            _anim.SetBool(IdleKey, !isMoving);
            _anim.SetBool(WalkingKey, isMoving);
            _anim.SetBool(GroundedKey, _isGrounded);
            _anim.SetBool(StunnedKey, false);
        }
    }

    private void CheckGrounded()
    {
        if (_enemy.Col == null) return;

        Physics2D.queriesStartInColliders = false;

        bool groundHit = Physics2D.BoxCast(
            _enemy.Col.bounds.center,
            _enemy.Col.size,
            0f,
            Vector2.down,
            0.1f,
            LayerMask.GetMask("Ground")
        );

        _isGrounded = groundHit;

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    public void Jumped()
    {
        _anim.SetTrigger(JumpKey);
    }

    private void TriggerAttack()
    {
        if (_enemy.IsStunned) return; // Don't trigger attack if stunned
        _anim.SetTrigger(AttackKey);
    }

    /// <summary>
    /// Guardian: start windup (Bool = true so animator stays in windup state until success/failed).
    /// </summary>
    private void TriggerGrabWindup()
    {
        if (_enemy.IsStunned) return; // Don't start windup if stunned
        _anim.SetBool(GrabWindupKey, true);
    }

    /// <summary>
    /// Guardian: grab hit the player. Clear windup bool and fire trigger so animator can transition to success state.
    /// </summary>
    private void TriggerGrabSuccess()
    {
        _anim.SetBool(GrabWindupKey, false);
        _anim.SetTrigger(GrabSuccessKey);
    }

    /// <summary>
    /// Guardian: grab missed. Clear windup bool and fire trigger so animator can transition to failed state.
    /// </summary>
    private void TriggerGrabFailed()
    {
        _anim.SetBool(GrabWindupKey, false);
        _anim.SetTrigger(GrabFailedKey);
    }

    /// <summary>
    /// Called by Enemy when dying. Plays death animation.
    /// </summary>
    public void TriggerDeath()
    {
        _anim.SetTrigger(DieKey);
    }

    private void OnEnemyStunned()
    {
        // Additional logic when enemy gets stunned (if needed)
        // Already handled in Update() loop
    }

    private void OnEnemyStunRecovered()
    {
        // Additional logic when enemy recovers from stun (if needed)
        // Already handled in Update() loop
    }

    private static readonly int EnemyIDKey = Animator.StringToHash("EnemyType");
    private static readonly int IdleKey = Animator.StringToHash("Idle");
    private static readonly int WalkingKey = Animator.StringToHash("Walking");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int AttackKey = Animator.StringToHash("Attack");
    private static readonly int DieKey = Animator.StringToHash("Die");
    private static readonly int StunnedKey = Animator.StringToHash("Stunned");
    private static readonly int GrabWindupKey = Animator.StringToHash("GrabWindup");
    private static readonly int GrabSuccessKey = Animator.StringToHash("GrabSuccess");
    private static readonly int GrabFailedKey = Animator.StringToHash("GrabFailed");
}