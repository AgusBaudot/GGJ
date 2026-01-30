using UnityEngine;

/// <summary>
/// VERY primitive player attack. PlayerController will call the corresponding attack. Logic is not handled here, only behavior.
/// Basic and Grab attack stats are in PlayerBaseStats
/// </summary>

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private PlayerProjectile _projectilePrefab;
    [SerializeField] private Transform _projectileSpawn;
    
    private float _lastShotTime;
    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
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
        //Dash-like attack.
        //Player launches forward for x duration with x speed.
        //Upon hitting an enemy, it will be stunned for x time.
        //When dash finishes (regardless of having hit an enemy), set horizontal velocity to 0.
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
        
        projectile.Init(data, Vector2.right * _controller.GetDirection);
    }

    private void DoGrab()
    {
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