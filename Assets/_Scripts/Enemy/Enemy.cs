using UnityEngine;

/// <summary>
/// Decides behavior during runtime. Drops MaskPickup upon death.
/// </summary>

public class Enemy : MonoBehaviour
{
    public EnemyData Data { get; private set; }

    private int _currentHp;
    private MaskManager _maskManager;
    private MaskSpawner _maskSpawner;

    public void Init(MaskManager manager, MaskSpawner spawner, EnemyData data)
    {
        Data = data;
        _maskManager = manager;
        _maskSpawner = spawner;
        
        _currentHp = Data.MaxHP;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Data.EnemySprite;
    }

    private void Update()
    {
        //Switch on BehaviorType and code accordingly
    }

    public void TakeDamage(int amount)
    {
        _currentHp -= amount;
        if (_currentHp <= 0)
            Die();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        _maskManager.ApplyDamage(Data.ContactDmg);
    }

    private void Die()
    {
        _maskSpawner.SpawnPickupMask(Data.DroppedMask, _maskManager, transform.position);
        Destroy(gameObject);
    }
}
