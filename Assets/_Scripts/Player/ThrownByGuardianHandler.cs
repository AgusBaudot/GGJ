using System.Collections;
using UnityEngine;

/// <summary>
/// Added to the player when thrown by a Guardian. Uses OnCollisionEnter2D to detect
/// landing (reliable) and applies contact damage, then re-enables control and removes itself.
/// </summary>
public class ThrownByGuardianHandler : MonoBehaviour
{
    private int _damageAmount;
    private Collider2D _ignoreCollider;
    private MaskManager _maskManager;
    private PlayerController _playerController;
    private PlayerInput _playerInput;
    private bool _handled;

    private const float TimeoutSeconds = 3f;

    public void Init(int damageAmount, Collider2D ignoreCollider)
    {
        _damageAmount = damageAmount;
        _ignoreCollider = ignoreCollider;
        _playerController = GetComponent<PlayerController>();
        _playerInput = GetComponent<PlayerInput>();
        _maskManager = _playerController.MaskManager;
    }

    private void Start()
    {
        StartCoroutine(Timeout());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_handled) return;
        
        if (_ignoreCollider != null && other.collider == _ignoreCollider) return;

        _handled = true;
        StopAllCoroutines();

        if (_maskManager != null)
            _maskManager.ApplyDamage(_damageAmount);

        ReEnableControl();
        Destroy(this);
    }

    private IEnumerator Timeout()
    {
        yield return new WaitForSeconds(TimeoutSeconds);
        if (_handled) yield break;
        _handled = true;
        ReEnableControl();
        Destroy(this);
    }

    private void ReEnableControl()
    {
        if (_playerInput != null) _playerInput.enabled = true;
        if (_playerController != null) _playerController.enabled = true;
    }
}
