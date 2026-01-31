using UnityEngine;

/// <summary>
/// Attached to an enemy when thrown voluntarily by the player. Applies damage to this enemy
/// and to any enemy hit on impact (same amount to both). Removes itself after first valid hit.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class ThrownEnemyController : MonoBehaviour
{
    private int _damageAmount;
    private bool _damageApplied;

    public void Init(int damageAmount)
    {
        _damageAmount = damageAmount;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_damageApplied) return;

        var self = GetComponent<Enemy>();
        if (self == null || !self.IsAlive) return;

        _damageApplied = true;

        // Damage the thrown enemy (this one)
        self.TakeDamage(_damageAmount);

        // If hit another enemy, damage them with the same amount
        if (other.gameObject.TryGetComponent(out Enemy otherEnemy) && otherEnemy.IsAlive)
            otherEnemy.TakeDamage(_damageAmount);

        Destroy(this);
    }
}
