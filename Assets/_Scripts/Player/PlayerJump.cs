using System;
using UnityEngine;

/// <summary>
/// Handles all jump mechanics including buffering, coyote time, and double jump
/// </summary>
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private PlayerBaseStats _stats;
    [SerializeField] private MaskManager _maskManager;

    public event Action Jumped;

    private float _time;
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _extraJump = true;
    private float _timeJumpWasPressed;
    private float _frameLeftGrounded = float.MinValue;
    private bool _isGrounded;
    private bool _canJump = true; // For burst/teleport locking
    private Vector2 _currentVelocity;

    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_isGrounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void Update()
    {
        _time += Time.deltaTime;
    }

    public void SetGrounded(bool grounded, float landingVelocity = 0)
    {
        bool wasGrounded = _isGrounded;
        _isGrounded = grounded;

        // Landed on ground
        if (!wasGrounded && grounded)
        {
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            _extraJump = true;
        }
        // Left the ground
        else if (wasGrounded && !grounded)
        {
            _frameLeftGrounded = _time;
        }
    }

    public void SetCanJump(bool canJump)
    {
        _canJump = canJump;
    }

    public bool GetEndedJumpEarly()
    {
        return _endedJumpEarly;
    }

    public void ProcessJumpInput(bool jumpDown, bool jumpHeld, Vector2 currentVelocity)
    {
        _currentVelocity = currentVelocity;

        if (jumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        // Check if player released jump early
        if (!_endedJumpEarly && !_isGrounded && !jumpHeld && currentVelocity.y > 0)
            _endedJumpEarly = true;
    }

    public Vector2 HandleJump(Vector2 velocity)
    {
        if (!_canJump) return velocity;

        // If player doesn't want to jump or didn't jump inside buffer time return
        if (!_jumpToConsume && !HasBufferedJump) return velocity;

        // If player is grounded or still has coyote time use first jump
        if (_isGrounded || CanUseCoyote)
        {
            velocity.y = ExecuteJump();
        }
        // If player has extra jump and it's enabled, use second jump and consume it
        else if (_extraJump && _maskManager.HasDoubleJump())
        {
            velocity.y = ExecuteJump();
            _extraJump = false;
        }

        // Consume jump ability anyways
        _jumpToConsume = false;

        return velocity;
    }

    private float ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        Jumped?.Invoke();
        return _stats.JumpPower;
    }
}
