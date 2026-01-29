using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// In charge of everything the player does. Variables are in PlayerBaseStats.cs
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("DELETE THIS")] [SerializeField]
    private DevToolsAbilities _devTools;

    [Header("NOT THIS")] [SerializeField] private PlayerBaseStats _stats;
    [SerializeField] private MaskManager _maskManager;

    public int GetDirection { get; private set; }

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private FrameInput _frameInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

    #region Interface

    public Vector2 FrameInput => _frameInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    #endregion

    private float _time;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            GrabDown = Input.GetButtonDown("Grab") || Input.GetKeyDown(KeyCode.E),
            PrimaryDown = Input.GetButtonDown("Primary") || Input.GetKeyDown(KeyCode.J),
            SecondaryDown = Input.GetButtonDown("Secondary") || Input.GetKeyDown(KeyCode.K)
        };

        if (_stats.SnapInput)
        {
            _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold
                ? 0
                : Mathf.Sign(_frameInput.Move.x);
            _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold
                ? 0
                : Mathf.Sign(_frameInput.Move.y);
        }

        //GetDirection is 1 when facing right and -1 when facing left.
        if (_frameInput.Move.x != 0)
            GetDirection = _frameInput.Move.x < 0
                ? -1
                : 1;

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        if (_frameInput.PrimaryDown)
        {
            _dashBuffered = true;
            _timeDashWasPressed = _time;
        }

        if (_frameInput.SecondaryDown)
        {
            _teleportBuffered = true;
            _timeTeleportPressed = _time;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDash();
        HandleTeleport();
        if (_isDashing)
        {
            ApplyDashMovement();
        }
        else
        {
            HandleDirection();
            HandleGravity();
        }

        ApplyMovement();
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down,
            _stats.GrounderDistance, _stats.GroundLayers);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up,
            _stats.GrounderDistance, _stats.GroundLayers);

        // Hit a Ceiling
        if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            _extraJump = true;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _extraJump = true;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

        //If player doesn't want to jump or didn't jump inside buffer time return.
        if (!_jumpToConsume && !HasBufferedJump) return;

        //If player is grounded or still has coyote time use first jump.
        if (_grounded || CanUseCoyote) ExecuteJump();

        //If player has extra jump and its enabled, use second jump and consue it.
        else if (_extraJump && _devTools._hasJump)
        {
            ExecuteJump();
            _extraJump = false;
        }

        //Consume jump ability anyways.
        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.JumpPower;
        Jumped?.Invoke();
    }

    #endregion

    #region Dash

    private float _dashEndTime;
    private bool _isDashing;
    private bool _canDash = true;
    private bool _dashToConsume;
    private Vector2 _dashDirection;
    private bool _dashBuffered;
    private float _timeDashWasPressed;

    private bool HasBufferedDash => _dashBuffered && _time < _timeDashWasPressed + _stats.DashBuffer;

    private void HandleDash()
    {
        if (_isDashing || !_canDash)
        {
            _dashBuffered = false;
            return;
        }
        
        if (!HasBufferedDash || !CanDash()) return;
        
        StartDash();
        _dashBuffered = false;
    }

    private void StartDash()
    {
        _isDashing = true;
        _canDash = false;

        _dashDirection = GetDashDirection();
        _frameVelocity = _dashDirection * _stats.DashSpeed;
        _frameVelocity.y = 0f;

        _dashEndTime = _time + _stats.DashDuration;
    }

    private void ApplyDashMovement()
    {
        _frameVelocity = _dashDirection * _stats.DashSpeed;

        if (_time >= _dashEndTime)
            EndDash();
    }

    private void EndDash()
    {
        _isDashing = false;

        _frameVelocity.x = 0f;
        
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return Helpers.GetWait(_stats.DashCooldown);
        _canDash = true;
    }

    private Vector2 GetDashDirection()
    {
        float x = _frameInput.Move.x != 0
            ? Mathf.Sign(_frameInput.Move.x)
            : GetDirection;
        return new Vector2(x, 0f);
    }

    private bool CanDash()
    {
        //If player is not dashing, can dash, and has dash ability, then he can dash
        return !_isDashing && _canDash && _devTools._hasDash;
    }

    #endregion

    #region Teleport

    private bool _teleportBuffered;
    private float _timeTeleportPressed;
    private bool _canTeleport = true;

    private bool HasBufferedTeleport =>
        _teleportBuffered && _time < _timeTeleportPressed + _stats.TeleportBuffer;

    private void HandleTeleport()
    {
        if (!HasBufferedTeleport || !_canTeleport || !_devTools._hasTeleport) return;

        Vector2 dir = new Vector2(GetDirection, 0f);
        Vector2 origin = _rb.position;
        float distance = _stats.TeleportDistance;
        
        //Check if space is free
        RaycastHit2D hit = Physics2D.CapsuleCast(
            origin,
            _col.size,
            _col.direction,
            0f,
            dir,
            distance,
            _stats.GroundLayers
        );

        Vector2 targetPos = hit
            ? hit.point - dir * 0.1f
            : origin + dir * distance;

        _rb.position = targetPos;
        _frameVelocity = Vector2.zero;
        
        _teleportBuffered = false;
        _canTeleport = false;

        StartCoroutine(TeleportCooldown());
    }

    private IEnumerator TeleportCooldown()
    {
        yield return Helpers.GetWait(_stats.TeleportCooldown);
        _canTeleport = true;
    }

    #endregion


    #region Horizontal

    private void HandleDirection()
    {
        if (_frameInput.Move.x == 0)
        {
            var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed,
                _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (!_grounded)
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0)
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;

            _frameVelocity.y = Mathf.MoveTowards(
                _frameVelocity.y,
                -_stats.MaxFallSpeed,
                inAirGravity * Time.fixedDeltaTime
            );
        }
        else if (_frameVelocity.y < 0f)
        {
            _frameVelocity.y = 0f;
        }
    }

    #endregion

    private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null)
            Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}

public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 Move;
    public bool PrimaryDown;
    public bool SecondaryDown;
    public bool GrabDown;
}

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
    public Vector2 FrameInput { get; }
}

/*
Platformer-ish:
Dash Speed:     25–30
Dash Duration:  0.15–0.20
Dash Cooldown:  0.6–1.0

Brawler-ish:
Dash Speed:     18–22
Dash Duration:  0.25–0.35
Dash Cooldown:  0.8–1.2
 */