using UnityEngine;

/// <summary>
/// Projectile fired by enemies. Damages player on hit.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    private MaskManager _maskManager;
    private RangedAttackData _data;
    private Vector2 _direction;
    private float _spawnTime;

    public void Init(RangedAttackData data, MaskManager manager, Vector2 direction)
    {
        _data = data;
        _maskManager = manager;
        _direction = direction.normalized;
        _spawnTime = Time.time;

        var renderer = GetComponentInChildren<SpriteRenderer>();
        if (renderer)
        {
            renderer.sprite = data.ProjectileSprite;
            renderer.flipX = direction.x < 0;
        }
        
        var anim = GetComponentInChildren<Animator>();
        if (anim && data.AnimatorOverride)
            anim.runtimeAnimatorController = data.AnimatorOverride;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * (_data.Velocity * Time.deltaTime));

        if (Time.time >= _spawnTime + _data.LifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit player
        if (collision.CompareTag("Player"))
        {
            _maskManager.ApplyDamage(_data.Damage);
            
            Destroy(gameObject);
        }
        // Hit environment
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}