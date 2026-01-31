using System.Collections;
using UnityEngine;

/// <summary>
/// Handles Acrobat enemy behavior - hopping movement with dash/retreat attacks
/// </summary>
public class AcrobatBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("Debug Settings")]
    [SerializeField] private bool _enableDebugLogs = true;
    [SerializeField] private bool _enableDebugGizmos = true;
    [SerializeField] private bool _showGroundCheck = true;
    [SerializeField] private bool _showStateInfo = true;

    private Enemy _enemy;
    private EnemyData _data;
    private Transform _player;
    private Rigidbody2D _rb;
    private BoxCollider2D _col;

    private bool _isGrounded;
    private bool _isDashing;
    private bool _canAttack = true;
    private Vector2 _dashDirection;
    private float _dashEndTime;
    private bool _cachedQueryStartInColliders;

    // Debug tracking
    private string _currentState = "Idle";
    private float _lastDistanceToPlayer;
    private Vector2 _lastVelocity;
    private int _hopCount = 0;
    private int _dashCount = 0;
    private int _retreatCount = 0;

    public void Initialize(Enemy enemy)
    {
        _enemy = enemy;
        _data = enemy.Data;
        _player = enemy.Player;
        _rb = enemy.Rb;
        _col = enemy.Col;

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        if (_enableDebugLogs)
        {
            Debug.Log($"<color=cyan>[ACROBAT INIT]</color> Enemy: {gameObject.name}\n" +
                      $"JumpForce (Idle): {_data.JumpForce}\n" +
                      $"ApproachHorizontalSpeed: {_data.ApproachHorizontalSpeed}\n" +
                      $"ApproachJumpForce: {_data.ApproachJumpForce}\n" +
                      $"DashSpeed: {_data.DashSpeed}\n" +
                      $"DashDuration: {_data.DashDuration}\n" +
                      $"DetectionDistance: {_data.DetectionDistance}\n" +
                      $"AttackDistance: {_data.AttackDistance}\n" +
                      $"AttackCooldown: {_data.AttackCooldown}");
        }
    }

    public void UpdateBehavior()
    {
        if (_player == null)
        {
            if (_enableDebugLogs)
                Debug.LogWarning($"<color=red>[ACROBAT]</color> Player reference is null!");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        _lastDistanceToPlayer = distanceToPlayer;

        string previousState = _currentState;

        // Within attack range - decide to dash or retreat
        if (distanceToPlayer <= _data.AttackDistance && _canAttack && !_isDashing)
        {
            _currentState = "Attacking";
            if (_enableDebugLogs && previousState != _currentState)
                Debug.Log($"<color=yellow>[ACROBAT]</color> ENTERING ATTACK RANGE! Distance: {distanceToPlayer:F2}");
            
            StartCoroutine(AttackDecision());
        }
        // Within detection range but outside attack range - approach player
        else if (distanceToPlayer <= _data.DetectionDistance && distanceToPlayer > _data.AttackDistance)
        {
            _currentState = "Approaching";
            if (_enableDebugLogs && previousState != _currentState)
                Debug.Log($"<color=green>[ACROBAT]</color> DETECTED PLAYER! Distance: {distanceToPlayer:F2} - Starting approach");
            
            ApproachPlayer();
        }
        // Outside detection range - idle hopping
        else if (distanceToPlayer > _data.DetectionDistance)
        {
            _currentState = "Idle";
            if (_enableDebugLogs && previousState != _currentState)
                Debug.Log($"<color=gray>[ACROBAT]</color> Player out of range. Distance: {distanceToPlayer:F2} - Idle hopping");
            
            IdleHop();
        }
        else if (_isDashing)
        {
            _currentState = "Dashing";
        }
        else if (!_canAttack)
        {
            _currentState = "Cooldown";
        }
    }

    public void FixedUpdateBehavior()
    {
        CheckGrounded();
        HandleDash();

        _lastVelocity = _rb.velocity;
    }

    public void OnStunned()
    {
        if (_enableDebugLogs)
            Debug.Log($"<color=orange>[ACROBAT]</color> STUNNED! Stopping all movement. Was dashing: {_isDashing}");

        // Stop any ongoing dash
        _isDashing = false;
        _rb.velocity = Vector2.zero;
        _currentState = "Stunned";
    }

    public void OnStunEnded()
    {
        if (_enableDebugLogs)
            Debug.Log($"<color=orange>[ACROBAT]</color> STUN ENDED! Resuming normal behavior");
        
        _currentState = "Idle";
    }

    #region Ground Detection

    private void CheckGrounded()
    {
        Physics2D.queriesStartInColliders = false;

        // Check if enemy is on ground using BoxCast
        bool groundHit = Physics2D.BoxCast(
            _col.bounds.center,
            _col.size,
            0f,
            Vector2.down,
            0.1f, // Small distance below collider
            LayerMask.GetMask("Ground")
        );

        bool wasGrounded = _isGrounded;

        // Landed on ground
        if (!_isGrounded && groundHit)
        {
            _isGrounded = true;
            if (_enableDebugLogs)
                Debug.Log($"<color=green>[ACROBAT]</color> LANDED! Velocity on landing: {_rb.velocity.y:F2}");
        }
        // Left the ground
        else if (_isGrounded && !groundHit)
        {
            _isGrounded = false;
            if (_enableDebugLogs)
                Debug.Log($"<color=blue>[ACROBAT]</color> LEFT GROUND! Launch velocity: {_rb.velocity.y:F2}");
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Movement

    private void IdleHop()
    {
        // Hop in place when grounded
        if (_isGrounded && !_isDashing)
        {
            _hopCount++;
            _rb.velocity = new Vector2(0, _data.JumpForce);
            
            if (_enableDebugLogs)
                Debug.Log($"<color=gray>[ACROBAT]</color> IDLE HOP #{_hopCount} | JumpForce: {_data.JumpForce}");
        }
    }

    private void ApproachPlayer()
    {
        // Hop toward player when grounded
        if (_isGrounded && !_isDashing)
        {
            Vector2 directionToPlayer = (_player.position - transform.position).normalized;
            Vector2 velocity = new Vector2(
                directionToPlayer.x * _data.ApproachHorizontalSpeed, 
                _data.ApproachJumpForce
            );
            _rb.velocity = velocity;
            
            _hopCount++;

            if (_enableDebugLogs)
                Debug.Log($"<color=green>[ACROBAT]</color> APPROACH HOP #{_hopCount}\n" +
                          $"Direction: {directionToPlayer}\n" +
                          $"Horizontal Speed: {velocity.x:F2} (base: {_data.ApproachHorizontalSpeed})\n" +
                          $"Vertical Force: {_data.ApproachJumpForce}\n" +
                          $"Final Velocity: {velocity}\n" +
                          $"Distance to Player: {_lastDistanceToPlayer:F2}");
        }
    }

    private void HandleDash()
    {
        if (_isDashing)
        {
            float timeRemaining = _dashEndTime - Time.time;

            if (Time.time >= _dashEndTime)
            {
                _isDashing = false;
                _rb.velocity = new Vector2(0, _rb.velocity.y);
                
                if (_enableDebugLogs)
                    Debug.Log($"<color=yellow>[ACROBAT]</color> DASH ENDED! Final velocity: {_rb.velocity}");
            }
            else
            {
                Vector2 dashVelocity = new Vector2(_dashDirection.x * _data.DashSpeed, _rb.velocity.y);
                _rb.velocity = dashVelocity;

                if (_enableDebugLogs && Time.frameCount % 10 == 0) // Log every 10 frames to avoid spam
                    Debug.Log($"<color=yellow>[ACROBAT]</color> DASHING... Time remaining: {timeRemaining:F2}s | Velocity: {dashVelocity}");
            }
        }
    }

    #endregion

    #region Attack

    private IEnumerator AttackDecision()
    {
        _canAttack = false;

        // 50/50 coin flip
        bool shouldDash = Random.value > 0.5f;

        if (_enableDebugLogs)
            Debug.Log($"<color=red>[ACROBAT]</color> ATTACK DECISION! Chose: {(shouldDash ? "DASH ATTACK" : "RETREAT BAIT")}");

        if (shouldDash)
        {
            _dashCount++;
            // Dash toward player
            Vector2 directionToPlayer = (_player.position - transform.position).normalized;
            
            if (_enableDebugLogs)
                Debug.Log($"<color=red>[ACROBAT]</color> DASH ATTACK #{_dashCount}\n" +
                          $"Direction: {directionToPlayer}\n" +
                          $"DashSpeed: {_data.DashSpeed}\n" +
                          $"Duration: {_data.DashDuration}");
            
            StartDash(directionToPlayer);
        }
        else
        {
            _retreatCount++;
            if (_enableDebugLogs)
                Debug.Log($"<color=cyan>[ACROBAT]</color> RETREAT BAIT #{_retreatCount} - Starting double jump away");
            
            // Double jump away from player (bait)
            yield return StartCoroutine(DoubleJumpRetreat());
        }

        if (_enableDebugLogs)
            Debug.Log($"<color=orange>[ACROBAT]</color> Attack cooldown started: {_data.AttackCooldown}s");

        // Cooldown before next attack decision
        yield return new WaitForSeconds(_data.AttackCooldown);
        
        _canAttack = true;
        
        if (_enableDebugLogs)
            Debug.Log($"<color=orange>[ACROBAT]</color> Attack cooldown ended - Ready to attack again");
    }

    private void StartDash(Vector2 direction)
    {
        _isDashing = true;
        _dashDirection = direction;
        _dashEndTime = Time.time + _data.DashDuration;
        _enemy.NotifyAttackTriggered();

        if (_enableDebugLogs)
            Debug.Log($"<color=red>[ACROBAT]</color> DASH STARTED!\n" +
                      $"Direction: {direction}\n" +
                      $"Speed: {_data.DashSpeed}\n" +
                      $"Will end at: {_dashEndTime:F2} (in {_data.DashDuration}s)");
    }

    private IEnumerator DoubleJumpRetreat()
    {
        Vector2 directionAwayFromPlayer = (transform.position - _player.position).normalized;

        if (_enableDebugLogs)
            Debug.Log($"<color=cyan>[ACROBAT]</color> RETREAT - First Jump\n" +
                      $"Direction: {directionAwayFromPlayer}\n" +
                      $"IsGrounded: {_isGrounded}\n" +
                      $"Current Velocity: {_rb.velocity}");

        // First jump up and away
        if (_isGrounded)
        {
            _rb.velocity = new Vector2(directionAwayFromPlayer.x * _data.ApproachHorizontalSpeed, _data.ApproachJumpForce);
            
            if (_enableDebugLogs)
                Debug.Log($"<color=cyan>[ACROBAT]</color> First jump executed! Velocity: {_rb.velocity}");
        }
        else
        {
            if (_enableDebugLogs)
                Debug.LogWarning($"<color=cyan>[ACROBAT]</color> First jump SKIPPED - Not grounded!");
        }

        // Wait a moment, then second jump
        yield return new WaitForSeconds(0.2f);

        if (_enableDebugLogs)
            Debug.Log($"<color=cyan>[ACROBAT]</color> RETREAT - Second Jump\n" +
                      $"IsGrounded: {_isGrounded}\n" +
                      $"Current Velocity: {_rb.velocity}");

        // Second jump (if still in air)
        if (!_isGrounded)
        {
            _rb.velocity = new Vector2(directionAwayFromPlayer.x * _data.ApproachHorizontalSpeed, _data.ApproachJumpForce);
            
            if (_enableDebugLogs)
                Debug.Log($"<color=cyan>[ACROBAT]</color> Second jump executed! Velocity: {_rb.velocity}");
        }
        else
        {
            if (_enableDebugLogs)
                Debug.LogWarning($"<color=cyan>[ACROBAT]</color> Second jump SKIPPED - Already grounded!");
        }
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmos()
    {
        if (!_enableDebugGizmos || _player == null) return;

        // Ground check visualization
        if (_showGroundCheck && _col != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector3 boxCenter = _col.bounds.center + Vector3.down * 0.1f;
            Gizmos.DrawWireCube(boxCenter, _col.size);
        }

        // Dash direction visualization
        if (_isDashing)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, _dashDirection * 2f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)_dashDirection * 2f, 0.2f);
        }

        // Velocity visualization
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, _rb != null ? (Vector3)_rb.velocity * 0.1f : Vector3.zero);
    }

    private void OnGUI()
    {
        if (!_showStateInfo || _player == null) return;

        // Create a debug panel
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
        
        if (screenPos.z > 0)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;

            string debugText = $"State: {_currentState}\n" +
                              $"Grounded: {_isGrounded}\n" +
                              $"Velocity: ({_rb.velocity.x:F1}, {_rb.velocity.y:F1})\n" +
                              $"Dist: {_lastDistanceToPlayer:F1}\n" +
                              $"CanAttack: {_canAttack}\n" +
                              $"Hops: {_hopCount} | Dashes: {_dashCount} | Retreats: {_retreatCount}";

            // Background
            GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
            GUI.Box(new Rect(screenPos.x - 60, Screen.height - screenPos.y - 60, 120, 100), "");
            
            // Text
            GUI.Label(new Rect(screenPos.x - 60, Screen.height - screenPos.y - 60, 120, 100), debugText, style);
        }
    }

    #endregion
}