using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Sneaky enemy behavior - ranged attacks with teleport retreat
/// </summary>
public class SneakyBehavior : MonoBehaviour, IEnemyBehavior
{
    private Enemy _enemy;
    private EnemyData _data;
    private Transform _player;
    private Rigidbody2D _rb;
    private BoxCollider2D _col;

    private Transform _projectileSpawn;
    private bool _canShoot = true;
    private bool _canTeleport = true;
    private bool _isTeleporting;
    private bool _isFacingRight = true;
    private bool _isGrounded;
    private bool _cachedQueryStartInColliders;

    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
        _data = enemy.Data;
        _player = enemy.Player;
        _rb = enemy.Rb;
        _col = enemy.Col;

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        // Get or create projectile spawn point
        _projectileSpawn = transform.Find("ProjectileSpawn");
        if (_projectileSpawn == null)
        {
            Debug.LogError($"[SNEAKY] No 'ProjectileSpawn' child found on {gameObject.name}! Please add one.");
        }
    }

    public void UpdateBehavior()
    {
        if (_player == null) return;

        // Don't do anything while teleporting
        if (_isTeleporting) return;

        UpdateFacing();

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Within attack range (detection range = attack range for ranged enemy)
        if (distanceToPlayer <= _data.DetectionDistance)
        {
            // Shoot at player
            if (_canShoot)
            {
                StartCoroutine(ShootAtPlayer());
            }

            // Teleport away if player is too close
            if (distanceToPlayer <= _data.FleeDistance && _canTeleport)
            {
                StartCoroutine(TeleportAway());
            }
        }
    }

    public void FixedUpdateBehavior()
    {
        CheckGrounded();
    }

    public void OnStunned()
    {
        // Sneaky can't teleport while stunned
        if (_isTeleporting)
        {
            StopAllCoroutines();
            _isTeleporting = false;
        }
    }

    public void OnStunEnded()
    {
        // Sneaky can resume normal behavior
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
        // Don't update facing while teleporting
        if (_isTeleporting) return;

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

        // Flip the projectile spawn point
        if (_projectileSpawn)
        {
            Vector3 spawnPos = _projectileSpawn.localPosition;
            spawnPos.x = Mathf.Abs(spawnPos.x) * (_isFacingRight ? 1 : -1);
            _projectileSpawn.localPosition = spawnPos;
        }
    }

    #endregion

    #region Attack

    private IEnumerator ShootAtPlayer()
    {
        _canShoot = false;

        // Fire projectile toward player
        if (_projectileSpawn != null && _data.ProjectilePrefab != null)
        {
            Vector2 directionToPlayer = (_player.GetComponent<Collider2D>().bounds.center - _projectileSpawn.position).normalized;

            var projectile = Instantiate(_data.ProjectilePrefab, _projectileSpawn.position, Quaternion.identity);
            var enemyProjectile = projectile.GetComponent<EnemyProjectile>();

            if (enemyProjectile != null)
                enemyProjectile.Init(_data.RangedAttackData, _enemy.MaskManager, directionToPlayer);
            else
                Debug.LogError($"[SNEAKY] Projectile prefab is missing EnemyProjectile component!");
        }

        // Cooldown
        yield return new WaitForSeconds(_data.AttackCooldown);
        _canShoot = true;
    }

    #endregion

    #region Teleport

    private IEnumerator TeleportAway()
    {
        // Get teleport target away from player
        int teleportDirection = _isFacingRight ? -1 : 1; // Opposite of facing (away from player)
        
        if (!GetTeleportTarget(teleportDirection, out Vector2 target))
        {
            // Failed to find valid target, start cooldown anyway
            yield return StartCoroutine(TeleportCooldown());
            yield break;
        }

        _isTeleporting = true;
        _canTeleport = false;

        // Optional: Teleport dissolve duration (visual telegraph)
        if (_data.TeleportDissolveDuration > 0)
        {
            yield return new WaitForSeconds(_data.TeleportDissolveDuration);
        }

        // Execute teleport
        ExecuteTeleport(target);

        // Optional: Teleport reappear duration
        if (_data.TeleportReappearDuration > 0)
        {
            yield return new WaitForSeconds(_data.TeleportReappearDuration);
        }

        _isTeleporting = false;

        // Start cooldown
        yield return StartCoroutine(TeleportCooldown());
    }

    private bool GetTeleportTarget(int direction, out Vector2 target)
    {
        target = Vector2.zero;

        Vector2 dir = new Vector2(direction, 0f);
        Vector2 origin = _col.bounds.center;
        float distance = _data.TeleportDistance;

        Physics2D.queriesStartInColliders = true;

        // Check for obstacles in the path
        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            _col.size * 0.9f,
            0f,
            dir,
            distance,
            LayerMask.GetMask("Ground")
        );

        // Determine teleport destination (stop at obstacle or go full distance)
        Vector2 potentialTarget = hit ? hit.centroid : origin + dir * distance;

        // Adjust for pivot (similar to player teleport)
        potentialTarget.y -= _col.bounds.center.y - _rb.position.y;

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;

        // Now check if there's ground beneath the potential target
        Physics2D.queriesStartInColliders = false;

        // Raycast downward from the potential target position
        float maxFallDistance = 5f; // How far down to check for ground
        RaycastHit2D groundCheck = Physics2D.Raycast(
            potentialTarget + Vector2.up * _col.bounds.extents.y, // Start from bottom of collider
            Vector2.down,
            maxFallDistance,
            LayerMask.GetMask("Ground")
        );

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;

        // If no ground found, teleport failed
        if (!groundCheck)
        {
            return false;
        }

        // Ground found! Adjust target to be on the ground
        target = potentialTarget;
        target.y = groundCheck.point.y + (_col.bounds.extents.y); // Position so enemy is standing on ground

        return true;
    }

    private void ExecuteTeleport(Vector2 targetPosition)
    {
        // Handle trail renderer if present
        var trail = GetComponentInChildren<TrailRenderer>();
        if (trail) trail.emitting = false;

        // Physics move
        _rb.position = targetPosition;
        _rb.velocity = Vector2.zero;
        transform.position = targetPosition;
        Physics2D.SyncTransforms();

        // Reset trail
        if (trail) trail.emitting = true;
    }

    private IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(_data.TeleportCooldown);
        _canTeleport = true;
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (_data == null || _col == null) return;

        // Draw teleport range when not on cooldown
        if (_canTeleport && !_isTeleporting)
        {
            Vector2 origin = _col.bounds.center;
            int direction = _isFacingRight ? -1 : 1; // Away from player
            Vector2 dir = new Vector2(direction, 0f);

            Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f); // Purple
            Gizmos.DrawRay(origin, dir * _data.TeleportDistance);
            Gizmos.DrawWireSphere(origin + dir * _data.TeleportDistance, 0.3f);
        }
    }

    #endregion
}