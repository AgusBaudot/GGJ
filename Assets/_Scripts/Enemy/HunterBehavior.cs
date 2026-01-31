using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Hunter enemy behavior - ranged attacks with backwards retreat
/// </summary>
public class HunterBehavior : MonoBehaviour, IEnemyBehavior
{
    private Enemy _enemy;
    private EnemyData _data;
    private Transform _player;
    private Rigidbody2D _rb;
    private BoxCollider2D _col;

    private Transform _projectileSpawn;
    private bool _canShoot = true;
    private bool _isFacingRight = true;
    private bool _isGrounded;
    private bool _ledgeDetected;
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
            Debug.LogError($"[HUNTER] No 'ProjectileSpawn' child found on {gameObject.name}! Please add one.");
        }
    }

    public void UpdateBehavior()
    {
        if (_player == null) return;

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

            // Flee if player is too close
            if (distanceToPlayer <= _data.FleeDistance)
            {
                RetreatFromPlayer();
            }
        }
    }

    public void FixedUpdateBehavior()
    {
        CheckGrounded();
        CheckLedge();
    }

    public void OnStunned()
    {
        // Stop movement when stunned
        _rb.velocity = new Vector2(0, _rb.velocity.y);
    }

    public void OnStunEnded()
    {
        // Hunter can resume normal behavior immediately
    }

    #region Ground & Ledge Detection

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

    private void CheckLedge()
    {
        if (!_isGrounded)
        {
            _ledgeDetected = false;
            return;
        }

        Physics2D.queriesStartInColliders = false;

        // Check ahead for ledge (no ground in front when moving backwards)
        float checkDistance = 0.5f;
        Vector2 checkDirection = _isFacingRight ? Vector2.left : Vector2.right; // Opposite of facing since we move backwards
        Vector2 checkPosition = (Vector2)transform.position + checkDirection * (_col.bounds.extents.x + checkDistance);

        bool groundAhead = Physics2D.Raycast(
            checkPosition,
            Vector2.down,
            _col.bounds.extents.y + 0.5f,
            LayerMask.GetMask("Ground")
        );

        _ledgeDetected = !groundAhead;

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Facing

    private void UpdateFacing()
    {
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
        // Flip the sprite
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

    #region Movement

    private void RetreatFromPlayer()
    {
        // Don't retreat if on a ledge or not grounded
        if (_ledgeDetected || !_isGrounded) return;

        // Move backwards (away from player)
        float retreatDirection = _isFacingRight ? -1 : 1; // Opposite of facing direction
        _rb.velocity = new Vector2(retreatDirection * _data.BackwardsSpeed, _rb.velocity.y);
    }

    #endregion

    #region Attack

    private IEnumerator ShootAtPlayer()
    {
        _canShoot = false;

        // Fire projectile toward player
        if (_projectileSpawn != null && _data.ProjectilePrefab != null)
        {
            Vector2 directionToPlayer = (_player.gameObject.GetComponent<Collider2D>().bounds.center - _projectileSpawn.position).normalized;

            var projectile = Instantiate(_data.ProjectilePrefab, _projectileSpawn.position, Quaternion.identity);
            var enemyProjectile = projectile.GetComponent<EnemyProjectile>();

            if (enemyProjectile != null)
                enemyProjectile.Init(_data.RangedAttackData, _enemy.MaskManager, directionToPlayer);
            else
                Debug.LogError($"[HUNTER] Projectile prefab is missing EnemyProjectile component!");
        }

        // Cooldown
        yield return new WaitForSeconds(_data.AttackCooldown);
        _canShoot = true;
    }

    #endregion
}