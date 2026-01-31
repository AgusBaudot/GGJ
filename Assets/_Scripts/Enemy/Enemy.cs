using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Orchestrates enemy systems. Delegates to specialized behavior components.
/// Variables are in EnemyData.
/// </summary>

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    public EnemyData Data { get; private set; }
    public MaskManager MaskManager { get; private set; }
    public bool IsAlive => _currentHp > 0;
    public bool IsStunned => _stunned;
    public Transform Player { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    public BoxCollider2D Col { get; private set; }

    /// <summary>
    /// Fired when the enemy performs an attack (contact, ranged, grab, dash, etc.). Used by EnemyAnimator.
    /// </summary>
    public event Action OnAttackTriggered;

    /// <summary>
    /// Fired when a windup attack starts (e.g. Guardian grab telegraph). Used by EnemyAnimator for GrabWindup.
    /// </summary>
    public event Action OnAttackWindupStarted;

    /// <summary>
    /// Fired when a windup attack succeeds (e.g. Guardian grab hit). Used by EnemyAnimator for GrabSuccess.
    /// </summary>
    public event Action OnAttackSucceeded;

    /// <summary>
    /// Fired when a windup attack fails (e.g. Guardian grab miss). Used by EnemyAnimator for GrabFailed.
    /// </summary>
    public event Action OnAttackFailed;

    private MaskSpawner _maskSpawner;
    private int _currentHp;
    private bool _stunned;

    // Behavior component
    private IEnemyBehavior _behavior;

    public void Init(MaskManager manager, MaskSpawner spawner, EnemyData data)
    {
        Data = data;
        MaskManager = manager;
        _maskSpawner = spawner;
        
        _currentHp = Data.MaxHP;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Data.EnemySprite;

        // Get components
        Rb = GetComponent<Rigidbody2D>();
        Col = GetComponent<BoxCollider2D>();

        // Find player
        Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (Player == null)
            Debug.LogError("Player not found! Make sure Player has 'Player' tag.");

        // Initialize behavior based on type
        InitializeBehavior();
    }

    private void InitializeBehavior()
    {
        switch (Data.Behavior)
        {
            case BehaviorType.Hunter:
                _behavior = gameObject.AddComponent<HunterBehavior>();
                break;
            case BehaviorType.Acrobat:
                _behavior = gameObject.AddComponent<AcrobatBehavior>();
                break;
            case BehaviorType.Guardian:
                _behavior = gameObject.AddComponent<GuardianBehavior>();
                break;
            case BehaviorType.Sneaky:
                _behavior = gameObject.AddComponent<SneakyBehavior>();
                break;
        }

        _behavior?.Initialize(this);
    }

    private void Update()
    {
        if (!IsAlive || _stunned) return;
        _behavior?.UpdateBehavior();
    }

    private void FixedUpdate()
    {
        if (!IsAlive || _stunned) return;
        _behavior?.FixedUpdateBehavior();
    }

    public void ApplyStun(float duration)
    {
        if (_stunned) return;
        _stunned = true;
        _behavior?.OnStunned();
        StartCoroutine(TickStun(duration));
    }
    
    private IEnumerator TickStun(float duration)
    {
        yield return Helpers.GetWait(duration);
        _stunned = false;
        _behavior?.OnStunEnded();
    }
    
    public void TakeDamage(int amount)
    {
        _currentHp -= amount;
        if (_currentHp <= 0)
            Die();
    }

    public void TryAttack()
    {
        if (_stunned) return;
        NotifyAttackTriggered();
        MaskManager.ApplyDamage(Data.ContactDmg);
    }

    /// <summary>
    /// Call from behaviors when performing an attack (ranged, grab, dash, etc.) so the animator can play the attack animation.
    /// </summary>
    public void NotifyAttackTriggered()
    {
        OnAttackTriggered?.Invoke();
    }

    /// <summary>
    /// Call from behaviors when starting a windup attack (e.g. Guardian grab telegraph). Animator can play windup → then success/failed.
    /// </summary>
    public void NotifyAttackWindupStarted()
    {
        OnAttackWindupStarted?.Invoke();
    }

    /// <summary>
    /// Call from behaviors when a windup attack succeeds (e.g. Guardian grab hit).
    /// </summary>
    public void NotifyAttackSucceeded()
    {
        OnAttackSucceeded?.Invoke();
    }

    /// <summary>
    /// Call from behaviors when a windup attack fails (e.g. Guardian grab miss).
    /// </summary>
    public void NotifyAttackFailed()
    {
        OnAttackFailed?.Invoke();
    }

    private void Die()
    {
        var enemyAnimator = GetComponentInChildren<EnemyAnimator>();
        if (enemyAnimator != null)
            StartCoroutine(DieRoutine(enemyAnimator));
        else
            Destroy(gameObject);
    }

    private IEnumerator DieRoutine(EnemyAnimator enemyAnimator)
    {
        enemyAnimator.TriggerDeath();
        yield return new WaitForSeconds(enemyAnimator.DeathAnimationDuration);
        _maskSpawner.SpawnPickupMask(Data.DroppedMask, transform.position);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (Data == null) return;
        
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, Data.DetectionDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Data.AttackDistance);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, Data.FleeDistance);
    }

}