using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Decides behavior during runtime. Drops MaskPickup upon death.
/// </summary>

public class Enemy : MonoBehaviour
{
    public EnemyData Data { get; private set; }
    public bool IsAlive => _currentHp > 0;

    private int _currentHp;
    private MaskManager _maskManager;
    private MaskSpawner _maskSpawner;
    private bool _stunned;

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
        switch (Data.Behavior)
        {
            case BehaviorType.Hunter:
                //Fire mask.
                break;
            case BehaviorType.Acrobat:
                //Movement mask.
                break;
            case BehaviorType.Guardian:
                //Tank mask.
                break;
            case BehaviorType.Sneaky:
                //Fog mask.
                break;
        }
    }

    public void ApplyStun(float duration)
    {
        _stunned = true;
        StartCoroutine(TickStun(duration));
    }
    
    private IEnumerator TickStun(float duration)
    {
        yield return Helpers.GetWait(duration);
        _stunned = false;
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
        _maskSpawner.SpawnPickupMask(Data.DroppedMask, transform.position);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, Data.DetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Data.att);
    }
}
