using UnityEngine;

/// <summary>
/// Tracks run stats: distance traveled (from player X) and enemies killed.
/// Singleton so enemies and UI can access it easily.
/// </summary>
public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    [SerializeField] private Transform _player;

    public float DistanceTraveled { get; private set; }
    public int EnemiesKilled { get; private set; }

    private float _lastPlayerX;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_player != null)
            _lastPlayerX = _player.position.x;
    }

    private void Update()
    {
        if (_player == null) return;

        float x = _player.position.x;
        DistanceTraveled += Mathf.Max(0f, x - _lastPlayerX);
        _lastPlayerX = x;
    }

    /// <summary>
    /// Call from enemies when they die (e.g. Enemy.Die() or death animation end).
    /// </summary>
    public void AddKill()
    {
        EnemiesKilled++;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
