using UnityEngine;

/// <summary>
/// Handles horizontal movement and gravity for the player
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerBaseStats _stats;

    private Rigidbody2D _rb;
    private Vector2 _velocity;
    private bool _isGrounded;
    private bool _endedJumpEarly;
    private bool _isControlLocked; // For burst/teleport

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
    }

    public void SetEndedJumpEarly(bool endedEarly)
    {
        _endedJumpEarly = endedEarly;
    }

    public void SetControlLocked(bool locked)
    {
        _isControlLocked = locked;
    }

    public void SetVelocity(Vector2 velocity)
    {
        _velocity = velocity;
    }

    public Vector2 GetVelocity()
    {
        return _velocity;
    }

    public void UpdateMovement(Vector2 moveInput)
    {
        if (_isControlLocked) return;

        HandleHorizontalMovement(moveInput.x);
        HandleGravity();
    }

    public void ApplyVelocity()
    {
        _rb.velocity = _velocity;
    }

    private void HandleHorizontalMovement(float horizontalInput)
    {
        if (horizontalInput == 0)
        {
            var deceleration = _isGrounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, horizontalInput * _stats.MaxSpeed,
                _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (!_isGrounded)
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _velocity.y > 0)
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;

            _velocity.y = Mathf.MoveTowards(
                _velocity.y,
                -_stats.MaxFallSpeed,
                inAirGravity * Time.fixedDeltaTime
            );
        }
        else if (_velocity.y < 0f)
        {
            _velocity.y = 0f;
        }
    }
}
