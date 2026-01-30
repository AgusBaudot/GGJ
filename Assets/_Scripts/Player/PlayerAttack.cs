using System.Collections;
using UnityEngine;

/// <summary>
/// VERY primitive player attack. PlayerController will call the corresponding attack. Logic is not handled here, only behavior.
/// </summary>

public class PlayerAttack : MonoBehaviour
{    
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
        Debug.LogWarning("Basic attack type not implemented yet.");
    }

    private void DoRanged()
    {
        Debug.LogWarning("Ranged attack type not implemented yet.");
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