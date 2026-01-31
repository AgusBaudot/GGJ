using UnityEngine;
using System.Collections; // Needed for IEnumerator

public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] private PlayerBaseStats _stats;

    private bool _canTeleport = true;
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;

    // We only need to expose if we are technically in the middle of a move
    public bool IsTeleporting { get; private set; }
    public bool CanTeleport => _canTeleport;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
    }

    // REMOVED: Update() and BufferTeleport() are no longer needed. 
    // The Controller determines when input happens.

    public bool GetTeleportTarget(int direction, out Vector2 target)
    {
        target = Vector2.zero;

        // FIX: Only check if the cooldown allows it. 
        // We removed the "hasBuffered" check because the Controller calls this explicitly.
        if (!_canTeleport) return false;

        Vector2 dir = new Vector2(direction, 0f);
        Vector2 origin = _col.bounds.center;
        float distance = _stats.TeleportDistance;

        bool cached = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = true;

        RaycastHit2D hit = Physics2D.CapsuleCast(
            origin, _col.size * 0.9f, _col.direction, 0f, dir, distance, _stats.GroundLayers
        );

        target = hit ? hit.centroid : origin + dir * distance;

        // Adjust for pivot
        target.y -= _col.bounds.center.y - _rb.position.y;

        Physics2D.queriesStartInColliders = cached;
        return true;
    }

    public void ExecuteTeleportMove(Vector2 targetPosition)
    {
        IsTeleporting = true;
        _canTeleport = false; // Lock cooldown immediately

        // 1. Handle specialized children (TrailRenderer)
        var trail = GetComponentInChildren<TrailRenderer>();
        if (trail) trail.emitting = false;

        // 2. Physics Move
        _rb.position = targetPosition;
        _rb.velocity = Vector2.zero;
        transform.position = targetPosition;
        Physics2D.SyncTransforms();

        // 3. Reset Trail
        if (trail) trail.emitting = true;

        // 4. Start internal cooldown
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(_stats.TeleportCooldown);
        _canTeleport = true;
        IsTeleporting = false;
    }
}