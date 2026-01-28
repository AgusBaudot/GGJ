using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Collider2D _attackCollider;
    private PlayerController _playerController;
    private WaitForSeconds _attackDuration = new(0.5f);
    
    public IEnumerator Attack()
    {
        _attackCollider ??= GetComponent<Collider2D>();
        
        _attackCollider.enabled = true;
        yield return _attackDuration;
        _attackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        _playerController ??= GetComponentInParent<PlayerController>();
        // _playerController.Attack(other.GetComponent<Enemy>());
        Debug.LogWarning("Missing attack method in player that damages enemies.");
    }
}