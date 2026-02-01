using UnityEngine;

public class ArmAnimator : MonoBehaviour
{
    [SerializeField] private PlayerAttack _playerAttack;

    private Animator _anim;
    private IPlayerController _player;
    private bool _grounded;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        if (_playerAttack == null)
            _playerAttack = GetComponentInParent<PlayerAttack>();
        _player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable()
    {
        if (_player != null)
            _player.GroundedChanged += OnGroundedChanged;
    }

    private void OnDisable()
    {
        if (_player != null)
            _player.GroundedChanged -= OnGroundedChanged;
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;
    }

    private void Update()
    {
        if (_anim == null || _playerAttack == null) return;

        bool holding = _playerAttack.IsHoldingEnemy;
        _anim.SetBool(HoldingEnemyKey, holding);

        if (holding && _player != null)
        {
            _anim.SetBool(GroundedKey, _grounded);
            _anim.SetBool(WalkingKey, _player.FrameInput.x != 0);
        }
    }

    /// <summary>
    /// Call when the player successfully grabs an enemy.
    /// </summary>
    public void PerformGrabAnimation()
    {
        if (_anim != null)
            _anim.SetTrigger(GrabAttackKey);
    }

    /// <summary>
    /// Call when the player throws the held enemy (voluntary or involuntary e.g. from damage).
    /// </summary>
    public void PerformThrowAnimation()
    {
        if (_anim != null)
            _anim.SetTrigger(ThrowAttackKey);
    }

    private static readonly int GrabAttackKey = Animator.StringToHash("GrabAttack");
    private static readonly int ThrowAttackKey = Animator.StringToHash("ThrowAttack");
    private static readonly int HoldingEnemyKey = Animator.StringToHash("HoldingEnemy");
    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int WalkingKey = Animator.StringToHash("Walking");
}
