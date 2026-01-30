using UnityEngine;

/// <summary>
/// VERY primitive animator example.
/// </summary>

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private MaskManager _maskManager;

    [Header("Particles")] 
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _doubleJumpParticles;
    [SerializeField] private ParticleSystem _dashParticles;

    [Header("Audio Clips")] [SerializeField]
    private AudioClip[] _footsteps;

    private AudioSource _source;
    private IPlayerController _player;
    private bool _grounded;
    private ParticleSystem.MinMaxGradient _currentGradient;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.Dashed += OnDashed;
        _player.Teleported += OnTeleported;
        _player.GroundedChanged += OnGroundedChanged;
        _player.Attacked += OnAttacked;

        if (_maskManager != null)
        {
            _maskManager.OnMaskEquipped += OnMaskEquipped;
            _maskManager.OnMaskBroken += OnMaskBroken;
            SyncMaskState();
        }

        _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.Dashed -= OnDashed;
        _player.Teleported -= OnTeleported;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.Attacked -= OnAttacked;

        if (_maskManager != null)
        {
            _maskManager.OnMaskEquipped -= OnMaskEquipped;
            _maskManager.OnMaskBroken -= OnMaskBroken;
        }

        _moveParticles.Stop();
    }

    private void Update()
    {
        if (_player == null) return;

        DetectGroundColor();

        HandleSpriteFlip();

        HandleCharacterWalk();
    }

    private void HandleSpriteFlip()
    {
        if (_player.FrameInput.x != 0) _sprite.flipX = _player.FrameInput.x < 0;
    }

    private void HandleCharacterWalk()
    {
        _anim.SetBool(IsWalking, _player.FrameInput.x != 0);
    }

    private void OnJumped()
    {
        _anim.SetTrigger(JumpKey);
        _anim.SetBool(GroundedKey, false);


        if (_grounded) // Avoid coyote
        {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);
            _jumpParticles.Play();
        }
        else
        {
            _doubleJumpParticles.Play();
        }
    }

    private void OnDashed()
    {
        _dashParticles.Play();
        _anim.SetTrigger(DashKey);
    }

    private void OnTeleported()
    {
        _anim.SetTrigger(TeleportKey);
        // Add teleport VFX here if desired (e.g. particles, screen effect)
    }

    private void OnMaskEquipped(MaskData data)
    {
        _anim.SetInteger(MaskEquippedKey, data != null ? data.MaskAnimationID : 0);
    }

    private void OnMaskBroken()
    {
        if (_maskManager != null && _maskManager.IsMaskless())
            _anim.SetInteger(MaskEquippedKey, 0);
    }

    private void SyncMaskState()
    {
        if (_maskManager.IsMaskless())
            _anim.SetInteger(MaskEquippedKey, 0);
        else if (_maskManager.CurrentMask != null)
            _anim.SetInteger(MaskEquippedKey, _maskManager.CurrentMask.Data.MaskAnimationID);
    }

    private void OnAttacked(AttackType type)
    {
        switch (type)
        {
            case AttackType.Basic:
                _anim.SetTrigger(BasicAttackKey);
                break;
            case AttackType.Ranged:
                _anim.SetTrigger(RangedAttackKey);
                break;
            case AttackType.Grab:
                _anim.SetTrigger(GrabAttackKey);
                break;
        }
    }

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;
        _anim.SetBool(GroundedKey, grounded);

        if (grounded)
        {
            DetectGroundColor();
            SetColor(_landParticles);

            // _anim.SetBool(GroundedKey, grounded);
            _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            _moveParticles.Play();

            _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            _landParticles.Play();
        }
        else
        {
            _moveParticles.Stop();
        }
    }

    private void DetectGroundColor()
    {
        var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

        if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;
        var color = r.color;
        _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
        SetColor(_moveParticles);
    }

    private void SetColor(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int DashKey = Animator.StringToHash("Dash");
    private static readonly int TeleportKey = Animator.StringToHash("Teleport");
    private static readonly int IsWalking = Animator.StringToHash("Walking");
    private static readonly int BasicAttackKey = Animator.StringToHash("BasicAttack");
    private static readonly int RangedAttackKey = Animator.StringToHash("RangedAttack");
    private static readonly int GrabAttackKey = Animator.StringToHash("GrabAttack");
    private static readonly int MaskEquippedKey = Animator.StringToHash("MaskEquipped");
}