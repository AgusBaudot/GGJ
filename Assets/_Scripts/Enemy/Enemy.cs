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

    private int _currentHp;
    private MaskSpawner _maskSpawner;
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
        MaskManager.ApplyDamage(Data.ContactDmg);
    }

    private void Die()
    {
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