using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private RangedAttackData _data;
    private Vector2 _direction;
    private float _spawnTime;

    public void Init(RangedAttackData data, Vector2 direction)
    {
        _data = data;
        _direction = direction;
        _spawnTime = Time.time;

        GetComponentInChildren<SpriteRenderer>().sprite = data.ProjectileSprite;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * (_data.Velocity * Time.deltaTime));

        if (Time.time >= _spawnTime + _data.LifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
            collision.GetComponent<Enemy>().TakeDamage(_data.Damage);
        
        Destroy(gameObject);
    }
}