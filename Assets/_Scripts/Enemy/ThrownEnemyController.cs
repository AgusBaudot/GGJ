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
    private AudioClip _impactSound;

    public void Init(int damageAmount, AudioClip audio)
    {
        _damageAmount = damageAmount;
        _impactSound = audio;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        SoundFXManager.instance.PlaySoundFXClip(_impactSound, transform, 1);
        
        if (_damageApplied) return;

        var self = GetComponent<Enemy>();
        if (self == null || !self.IsAlive) return;

        _damageApplied = true;

        // Damage the thrown enemy (this one)
        self.TakeDamage(_damageAmount);
        
        self.RemoveStun();

        // If hit another enemy, damage them with the same amount
        if (other.gameObject.TryGetComponent(out Enemy otherEnemy) && otherEnemy.IsAlive)
            otherEnemy.TakeDamage(_damageAmount);

        Destroy(this);
    }
}
