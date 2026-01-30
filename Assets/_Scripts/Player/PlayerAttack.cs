using System;
using UnityEngine;

/// <summary>
/// Handles player attack behaviors. PlayerController delegates attack calls here.
/// Basic and Grab attack stats are in PlayerBaseStats.
/// Fires AttackExecuted only when the attack actually executes (passes cooldown, etc.).
/// </summary>

public class PlayerAttack : MonoBehaviour
{
    public event Action<AttackType> AttackExecuted;

    [SerializeField] private PlayerBaseStats _stats;
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private PlayerProjectile _projectilePrefab;
    [SerializeField] private Transform _projectileSpawn;
    [SerializeField] private Transform _holdAnchor;

    private float _lastShotTime;
    private float _grabCooldownEndTime;
    private Enemy _heldEnemy;
    private PlayerInput _input;
    private Collider2D _playerCollider;

    public bool IsHoldingEnemy => _heldEnemy != null;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
        _playerCollider = GetComponent<Collider2D>();
        _maskManager.OnDamageReceived += OnDamageReceived;
    }

    private void OnDestroy()
    {
        _maskManager.OnDamageReceived -= OnDamageReceived;
    }

    public void TryAttack(AttackType type)
    {
        switch (type)
        {
            case AttackType.Basic:
                DoBasic();
                break;

            case AttackType.Ranged:
                DoRanged();
                break;

            case AttackType.Grab:
                DoGrab();
                break;
        }
    }

    private void DoBasic()
    {
        //Handled in PlayerBurst.cs
    }


    public void OnBurstHitEnemy(Enemy enemy, MovementDashData data)
    {
        if (!data.DealsDamage) return;
        
        enemy.TakeDamage((int)_maskManager.CurrentMask.Data.DmgModifier);

        if (enemy.IsAlive)
            enemy.ApplyStun(data.StunDuration);
    }

    private void DoRanged()
    {
        RangedAttackData data = _maskManager.GetCurrentRangedAttack();
        if (data == null) return;

        if (Time.time < _lastShotTime + 1f / data.FireRate)
            return;
        
        _lastShotTime = Time.time;

        var projectile = Instantiate(_projectilePrefab, _projectileSpawn.position, Quaternion.identity);
        
        projectile.Init(data, Vector2.right * _input.FacingDirection);

        AttackExecuted?.Invoke(AttackType.Ranged);
    }

    private void DoGrab()
    {
        if (_stats == null) return;

        if (_heldEnemy != null)
        {
            PerformVoluntaryThrow();
            return;
        }

        if (Time.time < _grabCooldownEndTime) return;

        var enemy = FindNearestEnemyInFront();
        if (enemy == null) return;

        GrabEnemy(enemy);
        AttackExecuted?.Invoke(AttackType.Grab);
    }

    private Enemy FindNearestEnemyInFront()
    {
        int facing = _input.FacingDirection;
        Vector2 origin = transform.position;
        float range = _stats.GrabAttackRange;

        var hits = Physics2D.OverlapCircleAll(origin, range);
        Enemy nearest = null;
        float nearestSqDist = range * range;

        foreach (var col in hits)
        {
            if (!col.TryGetComponent(out Enemy enemy) || !enemy.IsAlive) continue;
            float dx = enemy.transform.position.x - origin.x;
            if (dx * facing < 0) continue;
            float sqDist = (enemy.transform.position - (Vector3)origin).sqrMagnitude;
            if (sqDist < nearestSqDist)
            {
                nearestSqDist = sqDist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void GrabEnemy(Enemy enemy)
    {
        _heldEnemy = enemy;
        var rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        var enemyCol = enemy.GetComponent<Collider2D>();
        if (_playerCollider != null && enemyCol != null)
            Physics2D.IgnoreCollision(_playerCollider, enemyCol, true);
        enemy.transform.SetParent(_holdAnchor != null ? _holdAnchor : transform);
        enemy.transform.localPosition = Vector3.zero;
    }

    private void PerformVoluntaryThrow()
    {
        if (_heldEnemy == null) return;
        ThrowEnemy(_stats.ThrowDirection, _stats.ThrowForce, voluntary: true);
    }

    private void PerformAutoThrow()
    {
        if (_heldEnemy == null) return;
        ThrowEnemy(_stats.DropDirection, _stats.DropForce, voluntary: false);
    }

    private void ThrowEnemy(Vector2 direction, float force, bool voluntary)
    {
        var enemy = _heldEnemy;
        _heldEnemy = null;

        enemy.transform.SetParent(null);
        var rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            Vector2 dir = new Vector2(direction.x * _input.FacingDirection, direction.y).normalized;
            rb.AddForce(dir * force);
        }
        var enemyCol = enemy.GetComponent<Collider2D>();
        if (_playerCollider != null && enemyCol != null)
            Physics2D.IgnoreCollision(_playerCollider, enemyCol, false);

        if (voluntary && _maskManager.CurrentMask != null)
        {
            var thrown = enemy.gameObject.AddComponent<ThrownEnemyController>();
            thrown.Init((int)_maskManager.CurrentMask.Data.DmgModifier);
        }

        _grabCooldownEndTime = Time.time + _stats.GrabAttackCooldown;
    }

    private void OnDamageReceived(int amount)
    {
        if (_heldEnemy != null)
            PerformAutoThrow();
    }

    private void LateUpdate()
    {
        if (_heldEnemy == null) return;
        if (!_heldEnemy.IsAlive)
        {
            var enemyCol = _heldEnemy.GetComponent<Collider2D>();
            if (_playerCollider != null && enemyCol != null)
                Physics2D.IgnoreCollision(_playerCollider, enemyCol, false);
            _heldEnemy = null;
            return;
        }
        var anchor = _holdAnchor != null ? _holdAnchor : transform;
        _heldEnemy.transform.position = anchor.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(100);
        }
    }
}