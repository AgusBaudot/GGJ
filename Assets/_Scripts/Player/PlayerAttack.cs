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

    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private PlayerProjectile _projectilePrefab;
    [SerializeField] private Transform _projectileSpawn;
    
    private float _lastShotTime;
    private PlayerInput _input;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
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
        // When implemented, fire AttackExecuted?.Invoke(AttackType.Grab) here after successful grab
        Debug.LogWarning("Grab attack type not implemented yet.");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(100);
        }
    }
}