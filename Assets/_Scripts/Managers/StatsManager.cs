using System;
using UnityEngine;

/// <summary>
/// Tracks run stats: distance traveled (from player X) and enemies killed.
/// Singleton so enemies and UI can access it easily.
/// </summary>
public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    public event Action<int> OnEnemyKilled;

    [SerializeField] private Transform _player;

    public int DistanceTraveled { get; private set; }
    public int EnemiesKilled { get; private set; }

    private float _spawnPosition;
    private float _maxPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_player != null)
            _spawnPosition = _player.position.x;

        _maxPosition = _spawnPosition;
    }

    private void Update()
    {
        if (_player == null) return;

        _maxPosition = Mathf.Max(_maxPosition, _player.position.x);
        DistanceTraveled = (int)_maxPosition - (int)_spawnPosition;
    }

    /// <summary>
    /// Call from enemies when they die (e.g. Enemy.Die() or death animation end).
    /// </summary>
    public void AddKill()
    {
        EnemiesKilled++;
        OnEnemyKilled?.Invoke(EnemiesKilled);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
