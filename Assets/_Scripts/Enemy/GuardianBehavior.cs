using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Guardian enemy behavior - slow tank that grabs and throws the player
/// </summary>
public class GuardianBehavior : MonoBehaviour, IEnemyBehavior
{
    private Enemy _enemy;
    private EnemyData _data;
    private Transform _player;
    private Rigidbody2D _rb;
    private BoxCollider2D _col;
    private PlayerController _playerController;
    private Rigidbody2D _playerRb;
    private Collider2D _playerCollider;
    private PlayerInput _playerInput;
    private MaskManager _maskManager;

    private Transform _holdAnchor;
    private bool _isGrounded;
    private bool _isFacingRight = true;
    private bool _canGrab = true;
    private bool _isGrabbing;
    private bool _isHoldingPlayer;
    private Vector2 _grabDirection;
    private bool _cachedQueryStartInColliders;

    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
        _data = enemy.Data;
        _player = enemy.Player;
        _rb = enemy.Rb;
        _col = enemy.Col;

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        if (_player != null)
        {
            _playerController = _player.GetComponent<PlayerController>();
            _playerRb = _player.GetComponent<Rigidbody2D>();
            _playerCollider = _player.GetComponent<Collider2D>();
            _playerInput = _player.GetComponent<PlayerInput>();
            _maskManager = _player.GetComponent<MaskManager>();

            if (_playerController == null)
                Debug.LogError($"[GUARDIAN] Player is missing PlayerController component!");
        }

        // Get or create hold anchor
        _holdAnchor = transform.Find("HoldAnchor");
        if (_holdAnchor == null)
        {
            GameObject anchor = new GameObject("HoldAnchor");
            anchor.transform.SetParent(transform);
            anchor.transform.localPosition = _data.PlayerHoldOffset;
            _holdAnchor = anchor.transform;
        }
    }

    public void UpdateBehavior()
    {
        if (_player == null) return;

        // If holding player, don't do anything else
        if (_isHoldingPlayer) return;

        UpdateFacing();

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Always approach player (slow but constant)
        ApproachPlayer();

        // Within grab range - attempt grab
        if (distanceToPlayer <= _data.AttackDistance && _canGrab && !_isGrabbing)
        {
            StartCoroutine(AttemptGrab());
        }
    }

    public void FixedUpdateBehavior()
    {
        CheckGrounded();
    }

    public void OnStunned()
    {
        // Stop movement and cancel any grab attempt
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        
        if (_isGrabbing)
        {
            StopAllCoroutines();
            _isGrabbing = false;
            _canGrab = true;
        }

        // Release player if holding
        if (_isHoldingPlayer)
        {
            ReleasePlayer(false);
        }
    }

    public void OnStunEnded()
    {
        // Guardian can resume normal behavior
    }

    #region Ground Detection

    private void CheckGrounded()
    {
        Physics2D.queriesStartInColliders = false;

        bool groundHit = Physics2D.BoxCast(
            _col.bounds.center,
            _col.size,
            0f,
            Vector2.down,
            0.1f,
            LayerMask.GetMask("Ground")
        );

        _isGrounded = groundHit;

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Facing

    private void UpdateFacing()
    {
        // Don't update facing while grabbing or holding
        if (_isGrabbing || _isHoldingPlayer) return;

        // Always face the player
        bool shouldFaceRight = _player.position.x > transform.position.x;

        if (shouldFaceRight != _isFacingRight)
        {
            _isFacingRight = shouldFaceRight;
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        var spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer)
            spriteRenderer.flipX = !_isFacingRight;

        // Flip hold anchor
        if (_holdAnchor != null)
        {
            Vector3 anchorPos = _holdAnchor.localPosition;
            anchorPos.x = Mathf.Abs(anchorPos.x) * (_isFacingRight ? 1 : -1);
            _holdAnchor.localPosition = anchorPos;
        }
    }

    #endregion

    #region Movement

    private void ApproachPlayer()
    {
        // Don't move while grabbing or holding player
        if (_isGrabbing || _isHoldingPlayer || !_isGrounded) return;

        // Slow but constant approach
        float moveDirection = _isFacingRight ? 1 : -1;
        _rb.velocity = new Vector2(moveDirection * _data.MoveSpeed, _rb.velocity.y);
    }

    #endregion

    #region Grab Attack

    private IEnumerator AttemptGrab()
    {
        _canGrab = false;
        _isGrabbing = true;

        // Lock the grab direction to where player was initially seen
        _grabDirection = _isFacingRight ? Vector2.right : Vector2.left;

        // Stop moving during windup
        _rb.velocity = new Vector2(0, _rb.velocity.y);

        // Wait for telegraph/windup time
        yield return new WaitForSeconds(_data.GrabWindupTime);

        // Perform the grab check using BoxCast
        Physics2D.queriesStartInColliders = false;

        Vector2 grabCenter = (Vector2)transform.position + new Vector2(
            _grabDirection.x * _data.GrabOffset.x,
            _data.GrabOffset.y
        );

        Collider2D hit = Physics2D.OverlapBox(
            grabCenter,
            _data.GrabSize,
            0f,
            LayerMask.GetMask("Player")
        );

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;

        // Check if we grabbed the player (hit can be player root or child collider)
        if (hit != null && (hit.transform == _player || hit.transform.IsChildOf(_player)))
        {
            GrabPlayer();
            yield return new WaitForSeconds(_data.PlayerHoldDuration);
            ThrowPlayer();
        }
        else
        {
            // Missed - cooldown before next attempt
            _isGrabbing = false;
            yield return new WaitForSeconds(_data.GrabMissCooldown);
            _canGrab = true;
        }
    }

    private void GrabPlayer()
    {
        _isHoldingPlayer = true;

        //Disable player controller since it overwrites velocity.
        if (_playerController != null)
            _playerController.enabled = false;

        // Disable player physics
        if (_playerRb != null)
        {
            _playerRb.velocity = Vector2.zero;
            _playerRb.simulated = false;
        }

        // Disable player input
        if (_playerInput != null)
        {
            _playerInput.enabled = false;
        }

        // Ignore collision between player and guardian
        if (_playerCollider != null && _col != null)
        {
            Physics2D.IgnoreCollision(_playerCollider, _col, true);
        }

        // Parent player to hold anchor
        _player.SetParent(_holdAnchor);
        _player.localPosition = Vector3.zero;
    }

    private void ThrowPlayer()
    {
        if (_player == null) return;

        // Unparent player
        _player.SetParent(null);

        // Re-enable physics and apply throw as impulse (like PlayerAttack.ThrowEnemy)
        if (_playerRb != null)
        {
            _playerRb.simulated = true;
            Vector2 dir = new Vector2(_grabDirection.x * _data.ThrowDirection.x, _data.ThrowDirection.y).normalized;
            _playerRb.AddForce(dir * _data.ThrowForce, ForceMode2D.Impulse);
        }

        // Re-enable collision
        if (_playerCollider != null && _col != null)
        {
            Physics2D.IgnoreCollision(_playerCollider, _col, false);
        }

        _isHoldingPlayer = false;
        _isGrabbing = false;

        // Keep controller disabled until landing so throw arc isn't overwritten by movement
        StartCoroutine(MonitorPlayerLanding());

        // Cooldown before next grab
        StartCoroutine(GrabCooldown());
    }

    private IEnumerator MonitorPlayerLanding()
    {
        if (_playerInput == null || _playerCollider == null) yield break;

        // Wait a frame for physics to update
        yield return new WaitForFixedUpdate();

        // Wait until player collides with something
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(~0); // Check all layers

        Collider2D[] results = new Collider2D[10];
        float maxWaitTime = 3f; // Safety timeout
        float elapsedTime = 0f;

        while (elapsedTime < maxWaitTime)
        {
            int count = _playerCollider.OverlapCollider(filter, results);

            // Check if player is touching anything (excluding the guardian itself)
            for (int i = 0; i < count; i++)
            {
                if (results[i] != null && results[i] != _col)
                {
                    // Player landed - deal damage and re-enable controller + input
                    if (_maskManager != null)
                        _maskManager.ApplyDamage(_data.ContactDmg);

                    if (_playerInput != null)
                    {
                        _playerInput.enabled = true;
                    }
                    
                    if (_playerController != null)
                        _playerController.enabled = true;

                    yield break;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Safety: re-enable controller and input if timeout
        if (_playerInput != null) _playerInput.enabled = true;
        if (_playerController != null) _playerController.enabled = true;
    }

    private IEnumerator GrabCooldown()
    {
        yield return new WaitForSeconds(_data.AttackCooldown);
        _canGrab = true;
    }

    private void ReleasePlayer(bool dealDamage)
    {
        if (!_isHoldingPlayer || _player == null) return;

        // Unparent player
        _player.SetParent(null);

        // Re-enable physics
        if (_playerRb != null)
        {
            _playerRb.simulated = true;
        }

        // Re-enable input
        if (_playerInput != null)
        {
            _playerInput.enabled = true;
        }

        if (_playerController != null)
            _playerController.enabled = true;

        // Re-enable collision
        if (_playerCollider != null && _col != null)
        {
            Physics2D.IgnoreCollision(_playerCollider, _col, false);
        }

        // Deal damage if requested
        if (dealDamage && _maskManager != null)
        {
            _maskManager.ApplyDamage(_data.ContactDmg);
        }

        _isHoldingPlayer = false;
        _isGrabbing = false;
    }

    private void LateUpdate()
    {
        // Keep player at hold anchor position (similar to your grab implementation)
        if (_isHoldingPlayer && _player != null && _holdAnchor != null)
        {
            _player.position = _holdAnchor.position;
        }
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (_data == null) return;

        // Draw grab hitbox during windup
        if (_isGrabbing)
        {
            Vector2 grabDirection = _isFacingRight ? Vector2.right : Vector2.left;
            Vector2 grabCenter = (Vector2)transform.position + new Vector2(
                grabDirection.x * _data.GrabOffset.x,
                _data.GrabOffset.y
            );

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(grabCenter, _data.GrabSize);
        }

        // Draw hold anchor position
        if (_holdAnchor != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_holdAnchor.position, 0.2f);
        }
    }

    #endregion
}