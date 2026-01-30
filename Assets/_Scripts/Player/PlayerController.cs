using System;
using UnityEngine;

/// <summary>
/// Orchestrates all player systems. Delegates to specialized components for specific behaviors.
/// Variables are in PlayerBaseStats.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput), typeof(PlayerMovement), typeof(PlayerJump))]
[RequireComponent(typeof(PlayerBurst), typeof(PlayerTeleport))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("DEPENDENCIES")]
    [SerializeField] private PlayerBaseStats _stats;
    [SerializeField] private MaskManager _maskManager;

    // Components
    private PlayerInput _input;
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private PlayerBurst _burst;
    private PlayerTeleport _teleport;
    private PlayerAttack _playerAttack;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;

    // State
    private bool _grounded;
    private float _frameLeftGrounded = float.MinValue;
    private MaskPickup _nearbyMask;
    private bool _cachedQueryStartInColliders;

    public int GetDirection => _input.FacingDirection;

    #region Interface

    public Vector2 FrameInput => _input.CurrentInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Dashed;
    public event Action Teleported;
    public event Action<AttackType> Attacked;

    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _playerAttack = GetComponent<PlayerAttack>();

        _input = GetComponent<PlayerInput>();
        _movement = GetComponent<PlayerMovement>();
        _jump = GetComponent<PlayerJump>();
        _burst = GetComponent<PlayerBurst>();
        _teleport = GetComponent<PlayerTeleport>();

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        // Subscribe to component events (fire only when abilities actually execute)
        _jump.Jumped += OnJumped;
        _burst.Dashed += OnDashed;
        _teleport.Teleported += OnTeleported;
        _playerAttack.AttackExecuted += type => Attacked?.Invoke(type);
    }

    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        var input = _input.CurrentInput;

        // Mask pickup
        if (input.GrabDown && _nearbyMask != null)
        {
            TryGrabMask();
        }

        // Jump input
        _jump.ProcessJumpInput(input.JumpDown, input.JumpHeld, _movement.GetVelocity());

        // Primary attack - fire Attacked only when ability actually executes
        if (input.PrimaryDown)
        {
            var attack = _maskManager.GetCurrentAttack();

            // Delegate attack to PlayerAttack.cs (Ranged/Grab fire AttackExecuted internally)
            _playerAttack.TryAttack(attack);

            // Basic attack = tackle burst - fire Attacked only when burst actually starts
            if (attack == AttackType.Basic && !_burst.IsBursting && !_teleport.IsTeleporting)
            {
                if (_burst.TryStartBurst(_stats.TackleData, GetDirection))
                    Attacked?.Invoke(AttackType.Basic);
            }
        }

        // Secondary ability
        if (input.SecondaryDown)
        {
            switch (_maskManager.GetCurrentSecondary())
            {
                case SecondaryType.None:
                    return;

                case SecondaryType.Dash:
                    _burst.TryStartBurst(_stats.DashData, GetDirection);
                    break;

                case SecondaryType.Teleport:
                    _teleport.BufferTeleport();
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        Vector2 velocity = _movement.GetVelocity();

        // Handle special movement states
        if (_burst.IsBursting)
        {
            velocity = _burst.UpdateBurst();
            velocity.y = 0; // Keep burst horizontal
        }
        else if (!_teleport.IsTeleporting)
        {
            // Normal movement - apply jump first, then pass to movement before UpdateMovement
            velocity = _jump.HandleJump(velocity);
            _movement.SetVelocity(velocity); // Must set before UpdateMovement so jump isn't lost to gravity
            _movement.SetEndedJumpEarly(_jump.GetEndedJumpEarly());
            _movement.UpdateMovement(_input.CurrentInput.Move);
            velocity = _movement.GetVelocity();
        }
        else
        {
            // Teleporting - freeze movement
            velocity = Vector2.zero;
        }

        // Try to start teleport if buffered
        if (!_teleport.IsTeleporting && _maskManager.HasTeleport())
        {
            _teleport.TryStartTeleport(GetDirection);
        }

        _movement.SetVelocity(velocity);
        _movement.ApplyVelocity();
    }

    #region Collisions

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down,
            _stats.GrounderDistance, _stats.GroundLayers);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up,
            _stats.GrounderDistance, _stats.GroundLayers);

        // Hit a Ceiling
        if (ceilingHit)
        {
            Vector2 vel = _movement.GetVelocity();
            vel.y = Mathf.Min(0, vel.y);
            _movement.SetVelocity(vel);
        }

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _movement.SetGrounded(true);
            _jump.SetGrounded(true, Mathf.Abs(_movement.GetVelocity().y));
            GroundedChanged?.Invoke(true, Mathf.Abs(_movement.GetVelocity().y));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _movement.SetGrounded(false);
            _jump.SetGrounded(false);
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out MaskPickup mask)) return;
        Debug.LogWarning("Show 'E' sprite above player's head");
        _nearbyMask = mask;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out MaskPickup mask)) return;
        if (_nearbyMask == mask)
            _nearbyMask = null;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!_burst.IsBursting || _burst.CurrentBurst == null) return;

        if (other.gameObject.TryGetComponent(out Enemy enemy))
            _playerAttack.OnBurstHitEnemy(enemy, _burst.CurrentBurst);

        Debug.LogWarning("Differentiate somehow early burst end.");
        _burst.EndBurst();
    }

    #endregion

    private void TryGrabMask()
    {
        bool grabbed = _maskManager.AddMaskToStack(_nearbyMask.Data);
        if (!grabbed) return;

        Destroy(_nearbyMask.gameObject);
        _nearbyMask = null;
    }

    private void OnJumped()
    {
        Jumped?.Invoke();
    }

    private void OnDashed()
    {
        Dashed?.Invoke();
    }

    private void OnTeleported()
    {
        Teleported?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null)
            Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}
