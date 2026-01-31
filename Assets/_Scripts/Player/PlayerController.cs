using System;
using System.Collections;
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

    public MaskManager MaskManager => _maskManager;
    public int GetDirection => _input.FacingDirection;

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
    private bool _cachedQueryStartInColliders;
    private bool _isTeleportingSequence; //Locks input and physics during the animation.
    private float _frameLeftGrounded = float.MinValue;
    private MaskPickup _nearbyMask;

    #region Interface

    public Vector2 FrameInput => _input.CurrentInput.Move;
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    public event Action Dashed;
    public event Action TeleportStarted;
    public event Action TeleportEnded;
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
        _playerAttack.AttackExecuted += type => Attacked?.Invoke(type);
    }

    private void Update()
    {
        //Block input processing if player is in the middle of the teleport sequence
        if (_isTeleportingSequence) return;

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
                    if (!_teleport.IsTeleporting)
                        StartCoroutine(TeleportSequence());
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
        else if (_isTeleportingSequence)
        {
            velocity = Vector2.zero;
        }
        else
        {
            // Normal movement - apply jump first, then pass to movement before UpdateMovement
            velocity = _jump.HandleJump(velocity);
            _movement.SetVelocity(velocity); // Must set before UpdateMovement so jump isn't lost to gravity
            _movement.SetEndedJumpEarly(_jump.GetEndedJumpEarly());
            _movement.UpdateMovement(_input.CurrentInput.Move);
            velocity = _movement.GetVelocity();
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
        //Player and enemy collide
        //if player is tackling, player does damage to enemy and stuns him
        //else
        //enemy does damage to player

        //First ensure collision is with enemy.
        if (!other.gameObject.TryGetComponent(out Enemy enemy)) return;

        //If player isn't tackling, damage him.
        if (!_burst.IsBursting || _burst.CurrentBurst == null)
        {
            enemy.TryAttack();
        }
        //If player was tackling, damage and stun enemy
        else
        {
            _playerAttack.OnBurstHitEnemy(enemy, _burst.CurrentBurst);
            Debug.LogWarning("Differentiate somehow early burst end.");
            _burst.EndBurst();
        }

        // if (other.gameObject.TryGetComponent(out Enemy enemy))
        // _playerAttack.OnBurstHitEnemy(enemy, _burst.CurrentBurst);
    }

    #endregion

    private IEnumerator TeleportSequence()
    {
        if (!_teleport.GetTeleportTarget(GetDirection, out Vector2 target))
            yield break;

        _isTeleportingSequence = true;

        TeleportStarted?.Invoke(); // Signals Animator to play "Dissolve"

        yield return new WaitForSeconds(_stats.TeleportDissolveDuration);

        yield return new WaitForSeconds(_stats.TeleportDuration);

        _teleport.ExecuteTeleportMove(target);

        yield return null;

        TeleportEnded?.Invoke(); // Signals Animator to play "Reappear"

        yield return new WaitForSeconds(_stats.TeleportReappearDuration);

        _isTeleportingSequence = false;
    }

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null)
            Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}
