using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Links an enemy prefab and its data to a SpawnType for use by EnemySpawner.
/// </summary>
[System.Serializable]
public struct EnemySpawnEntry
{
    public SpawnType SpawnType;
    public GameObject EnemyPrefab;
    public EnemyData EnemyData;
}

/// <summary>
/// Spawns enemies in chunks based on distance and chunk spawn points.
/// Called by LevelGenerator when a new ahead chunk is instantiated.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MaskManager _maskManager;
    [SerializeField] private MaskSpawner _maskSpawner;
    [SerializeField] private PlayerBaseStats _stats;

    [Header("Enemy config (prefab + data per type)")]
    [SerializeField] private EnemySpawnEntry[] _enemyEntries;

    [Header("Difficulty pacing")]
    [Tooltip("X = distance traveled, Y = number of enemies to spawn in this chunk. Default: slow start, gentle ramp (fair pacing).")]
    [SerializeField] private AnimationCurve _enemiesPerChunkByDistance = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0.006f),
        new Keyframe(60f, 0.4f, 0.005f, 0.005f),
        new Keyframe(200f, 1f, 0.004f, 0.004f),
        new Keyframe(500f, 1.8f, 0.003f, 0.003f),
        new Keyframe(1000f, 2.5f, 0.002f, 0.002f),
        new Keyframe(2000f, 3.2f, 0.001f, 0.001f)
    );

    [Header("Overflow")]
    [Tooltip("When reusing a spawn point, random offset applied to avoid stacking. Max absolute per axis.")]
    [SerializeField] private Vector3 _overflowOffsetMax = new Vector3(0.5f, 0.5f, 0f);

    private Dictionary<SpawnType, EnemySpawnEntry> _entriesByType;

    private void Awake()
    {
        _entriesByType = new Dictionary<SpawnType, EnemySpawnEntry>();
        if (_enemyEntries != null)
        {
            foreach (var e in _enemyEntries)
                _entriesByType[e.SpawnType] = e;
        }
    }

    /// <summary>
    /// Called by LevelGenerator immediately after instantiating a new ahead chunk.
    /// </summary>
    /// <param name="chunkRoot">The chunk GameObject (root of the chunk prefab instance).</param>
    /// <param name="currentDistance">Distance traveled (e.g. player X) for difficulty curve.</param>
    public void SpawnEnemiesInChunk(GameObject chunkRoot, float currentDistance)
    {
        if (chunkRoot == null || _maskManager == null || _maskSpawner == null || _enemyEntries == null || _enemyEntries.Length == 0)
            return;

        Transform spawnsRoot = chunkRoot.transform.Find("Spawns");
        if (spawnsRoot == null)
            return;

        EnemySpawnPoint[] allPoints = spawnsRoot.GetComponentsInChildren<EnemySpawnPoint>(true);
        if (allPoints == null || allPoints.Length == 0)
            return;

        int enemyCount = Mathf.Max(0, Mathf.RoundToInt(_enemiesPerChunkByDistance.Evaluate(currentDistance)));
        if (enemyCount == 0)
            return;

        // Only consider points that have at least one allowed type we have an entry for
        var validPoints = new List<EnemySpawnPoint>();
        foreach (var pt in allPoints)
        {
            if (pt.AllowedTypes == null || pt.AllowedTypes.Length == 0) continue;
            foreach (var t in pt.AllowedTypes)
            {
                if (_entriesByType.ContainsKey(t))
                {
                    validPoints.Add(pt);
                    break;
                }
            }
        }
        if (validPoints.Count == 0)
            return;

        // Track how many times we've used each point this batch (for overflow offset)
        var usedCountByPoint = new Dictionary<EnemySpawnPoint, int>();

        for (int i = 0; i < enemyCount; i++)
        {
            EnemySpawnPoint chosen = validPoints[Random.Range(0, validPoints.Count)];
            SpawnType type = PickRandomAllowedType(chosen);
            if (!_entriesByType.TryGetValue(type, out EnemySpawnEntry entry) || entry.EnemyPrefab == null || entry.EnemyData == null)
                continue;

            int useIndex = usedCountByPoint.TryGetValue(chosen, out int c) ? c : 0;
            usedCountByPoint[chosen] = c + 1;

            Vector3 position = chosen.transform.position;
            if (useIndex > 0)
            {
                position += new Vector3(
                    Random.Range(-_overflowOffsetMax.x, _overflowOffsetMax.x),
                    Random.Range(-_overflowOffsetMax.y, _overflowOffsetMax.y),
                    Random.Range(-_overflowOffsetMax.z, _overflowOffsetMax.z)
                );
            }

            GameObject go = Instantiate(entry.EnemyPrefab, new Vector2(position.x, _overflowOffsetMax.y), Quaternion.identity, chunkRoot.transform);
            Enemy enemy = go.GetComponent<Enemy>();
            if (enemy != null)
                enemy.Init(_maskManager, _maskSpawner, entry.EnemyData, _stats);
        }
    }

    /// <summary>
    /// Picks one of the spawn point's allowed types at random with equal chance (only types we have entries for).
    /// </summary>
    private SpawnType PickRandomAllowedType(EnemySpawnPoint point)
    {
        if (point.AllowedTypes == null || point.AllowedTypes.Length == 0)
            return SpawnType.Fire;
        var eligible = new List<SpawnType>();
        foreach (var t in point.AllowedTypes)
        {
            if (_entriesByType.ContainsKey(t))
                eligible.Add(t);
        }
        if (eligible.Count == 0)
            return SpawnType.Fire;
        return eligible[Random.Range(0, eligible.Count)];
    }
}
