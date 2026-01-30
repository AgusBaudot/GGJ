using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles teleport mechanics
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] private PlayerBaseStats _stats;

    public event Action Teleported;

    private float _time;
    private bool _teleportBuffered;
    private bool _canTeleport = true;
    private bool _isTeleporting;
    private float _timeTeleportPressed;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private SpriteRenderer _renderer;

    public bool IsTeleporting => _isTeleporting;

    private bool HasBufferedTeleport =>
        _teleportBuffered && _time < _timeTeleportPressed + _stats.TeleportBuffer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        _time += Time.deltaTime;
    }

    public void BufferTeleport()
    {
        _teleportBuffered = true;
        _timeTeleportPressed = _time;
    }

    public bool TryStartTeleport(int direction)
    {
        if (_isTeleporting) return false;
        if (!HasBufferedTeleport || !_canTeleport) return false;

        _teleportBuffered = false;
        _canTeleport = false;

        Vector2 dir = new Vector2(direction, 0f);
        Vector2 origin = _col.bounds.center;
        float distance = _stats.TeleportDistance;

        bool cached = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = true;

        // Check if space is free
        RaycastHit2D hit = Physics2D.CapsuleCast(
            origin,
            _col.size * 0.9f,
            _col.direction,
            0f,
            dir,
            distance,
            _stats.GroundLayers
        );

        Vector2 teleportTarget = hit
            ? hit.centroid
            : origin + dir * distance;

        teleportTarget.y -= _col.bounds.center.y - _rb.position.y;

        Physics2D.queriesStartInColliders = cached;

        if (!_renderer)
            _renderer = GetComponentInChildren<SpriteRenderer>();

        StartCoroutine(TeleportRoutine(teleportTarget));
        return true;
    }

    private IEnumerator TeleportRoutine(Vector2 targetPosition)
    {
        _isTeleporting = true;
        Teleported?.Invoke();

        // 1. Kill both visuals and physics immediately
        _renderer.enabled = false;
        _rb.velocity = Vector2.zero;

        var trail = GetComponentInChildren<TrailRenderer>();
        if (trail) trail.emitting = false;

        yield return Helpers.GetWait(_stats.TeleportDuration);

        // 2. Move the Rigidbody
        _rb.position = targetPosition;
        _rb.velocity = Vector2.zero;

        // 3. Force the Transform to match the Rigidbody position now
        transform.position = targetPosition;

        // 4. Force physics to catch up immediately
        Physics2D.SyncTransforms();

        // 5. Wait two frames for camera to catch up
        yield return null;
        yield return null;

        _renderer.enabled = true;
        _isTeleporting = false;

        if (trail) trail.emitting = true;

        StartCoroutine(TeleportCooldown());
    }

    private IEnumerator TeleportCooldown()
    {
        yield return Helpers.GetWait(_stats.TeleportCooldown);
        _canTeleport = true;
    }
}
