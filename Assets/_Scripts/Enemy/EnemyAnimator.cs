using UnityEngine;

/// <summary>
/// Controls the enemy Animator based on state: idle (player out of range), run (moving), jump (in air), attack (trigger), die (trigger).
/// Guardian-specific: GrabWindup (trigger), GrabSuccess (trigger), GrabFailed (trigger) for windup → success/failed flow.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Duration of death animation before the enemy is destroyed.")]
    [SerializeField] private float _deathAnimationDuration = 0.5f;
    [Tooltip("Horizontal speed threshold to consider the enemy 'running'.")]
    [SerializeField] private float _runSpeedThreshold = 0.1f;

    private Enemy _enemy;
    private Animator _anim;
    private bool _isGrounded;
    private bool _cachedQueryStartInColliders;

    public float DeathAnimationDuration => _deathAnimationDuration;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
        _anim = GetComponent<Animator>();
        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void OnEnable()
    {
        if (_enemy != null)
        {
            _enemy.OnAttackTriggered += TriggerAttack;
            _enemy.OnAttackWindupStarted += TriggerGrabWindup;
            _enemy.OnAttackSucceeded += TriggerGrabSuccess;
            _enemy.OnAttackFailed += TriggerGrabFailed;
        }
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnAttackTriggered -= TriggerAttack;
            _enemy.OnAttackWindupStarted -= TriggerGrabWindup;
            _enemy.OnAttackSucceeded -= TriggerGrabSuccess;
            _enemy.OnAttackFailed -= TriggerGrabFailed;
        }
    }

    private void Update()
    {
        if (_enemy == null || !_enemy.IsAlive || _enemy.Data == null) return;

        CheckGrounded();

        // Idle: true when not moving (e.g. Guardian during windup). Mutually exclusive with Walking.
        bool isMoving = Mathf.Abs(_enemy.Rb.velocity.x) > _runSpeedThreshold;
        _anim.SetBool(IdleKey, !isMoving);

        // Walking: true when moving horizontally (matches Player / old Guardian "Walking" param).
        _anim.SetBool(WalkingKey, isMoving);

        // Grounded: true when on ground. Animator transitions to Jump when false (matches Player).
        _anim.SetBool(GroundedKey, _isGrounded);
    }

    private void CheckGrounded()
    {
        if (_enemy.Col == null) return;

        Physics2D.queriesStartInColliders = false;

        bool groundHit = Physics2D.BoxCast(
            _enemy.Col.bounds.center,
            _enemy.Col.size,
            0f,
            Vector2.down,
            0.1f,
            LayerMask.GetMask("Ground")
        );

        _isGrounded = groundHit;

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private void TriggerAttack()
    {
        _anim.SetTrigger(AttackKey);
    }

    /// <summary>
    /// Guardian: start windup (Bool = true so animator stays in windup state until success/failed).
    /// </summary>
    private void TriggerGrabWindup()
    {
        _anim.SetBool(GrabWindupKey, true);
    }

    /// <summary>
    /// Guardian: grab hit the player. Clear windup bool and fire trigger so animator can transition to success state.
    /// </summary>
    private void TriggerGrabSuccess()
    {
        _anim.SetBool(GrabWindupKey, false);
        _anim.SetTrigger(GrabSuccessKey);
    }

    /// <summary>
    /// Guardian: grab missed. Clear windup bool and fire trigger so animator can transition to failed state.
    /// </summary>
    private void TriggerGrabFailed()
    {
        _anim.SetBool(GrabWindupKey, false);
        _anim.SetTrigger(GrabFailedKey);
    }

    /// <summary>
    /// Called by Enemy when dying. Plays death animation.
    /// </summary>
    public void TriggerDeath()
    {
        _anim.SetTrigger(DieKey);
    }

    private static readonly int IdleKey = Animator.StringToHash("Idle");
    private static readonly int WalkingKey = Animator.StringToHash("Walking");
    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int AttackKey = Animator.StringToHash("Attack");
    private static readonly int DieKey = Animator.StringToHash("Die");
    private static readonly int GrabWindupKey = Animator.StringToHash("GrabWindup");
    private static readonly int GrabSuccessKey = Animator.StringToHash("GrabSuccess");
    private static readonly int GrabFailedKey = Animator.StringToHash("GrabFailed");
}
