using System;
using UnityEngine;

/// <summary>
/// Handles burst movement mechanics (dash and tackle attacks)
/// </summary>
/// 
public class PlayerBurst : MonoBehaviour
{
    public event Action Dashed;

    private float _time;
    private bool _isBursting;
    private float _burstEndTime;
    private float _burstCooldownEndTime;
    private MovementDashData _currentBurst;
    private Vector2 _burstDirection;

    public bool IsBursting => _isBursting;
    public MovementDashData CurrentBurst => _currentBurst;

    private void Update()
    {
        _time += Time.deltaTime;
    }

    public bool TryStartBurst(MovementDashData data, int direction)
    {
        if (_isBursting) return false;
        if (_time < _burstCooldownEndTime) return false;

        _currentBurst = data;
        _isBursting = true;

        if (!data.DealsDamage)
            Dashed?.Invoke();

        _burstDirection = Vector2.right * direction;
        _burstEndTime = _time + data.Duration;
        _burstCooldownEndTime = _burstEndTime + data.Cooldown;

        return true;
    }

    public Vector2 UpdateBurst()
    {
        if (!_isBursting) return Vector2.zero;

        if (_time >= _burstEndTime)
        {
            EndBurst();
            return Vector2.zero;
        }

        return _burstDirection * _currentBurst.Speed;
    }

    public void EndBurst()
    {
        _isBursting = false;
        _currentBurst = null;
    }
}
