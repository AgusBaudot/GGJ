using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LevelGenerator : MonoBehaviour
{
    public const float ChunkSizeX = 15f;
    public static LevelGenerator Instance { get; private set; }

    [SerializeField] private Transform _player;
    [SerializeField] private GameObject[] _chunkPrefabs;
    [SerializeField] private EnemySpawner _enemySpawner;

    private readonly List<Chunk> _activeChunks = new List<Chunk>(3);
    private Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (_chunkPrefabs == null || _chunkPrefabs.Length == 0) return;

        // Calculate where the player is on the grid
        float currentGridX = 0f;
        if (_player != null)
        {
             // This snaps the player to the nearest chunk center (0, 15, 30...)
             currentGridX = Mathf.Round(_player.position.x / ChunkSizeX) * ChunkSizeX;
        }

        // Spawn Behind (-15), Current (0), Ahead (+15) relative to player
        for (int i = -1; i <= 1; i++)
        {
            float x = currentGridX + (i * ChunkSizeX);
            Chunk chunk = GetOrCreateChunk(x);
            _activeChunks.Add(chunk);
        }
    }

    public void OnPlayerEnteredChunk(Chunk enteredChunk)
    {
        if (enteredChunk == null || _activeChunks.Count != 3) return;

        // 1. Identify if we really moved forward
        int index = _activeChunks.IndexOf(enteredChunk);
        if (index != 2) return; // Only trigger when entering the "Ahead" chunk

        // 2. Move the window forward
        RecycleChunk(_activeChunks[0]);
        _activeChunks.RemoveAt(0);

        // 3. Calculate Next X using GRID MATH (Fixes the 210m drift)
        // We take the current lead chunk's position, divide by 15, round it, then add 15.
        // This effectively "snaps" it to the perfect integer coordinate.
        float lastChunkX = _activeChunks[1].transform.position.x;
        float nextX = (Mathf.Round(lastChunkX / ChunkSizeX) * ChunkSizeX) + ChunkSizeX;

        Chunk newAhead = GetOrCreateChunk(nextX);
        _activeChunks.Add(newAhead);

        // 4. Notify Spawner
        if (_enemySpawner != null)
        {
            _enemySpawner.SpawnEnemiesInChunk(newAhead.gameObject, nextX);
        }
    }

    private Chunk GetOrCreateChunk(float worldX)
    {
        GameObject prefab = _chunkPrefabs[Random.Range(0, _chunkPrefabs.Length)];

        if (!_pool.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pool[prefab] = queue;
        }

        GameObject go = null;
        while (queue.Count > 0)
        {
            go = queue.Dequeue();
            if (go != null) break;
        }
        
        if (go == null)
        {
            go = Instantiate(prefab);
            if (go.GetComponent<Chunk>() == null) go.AddComponent<Chunk>();
        }
        else
        {
            go.SetActive(true);
        }

        Chunk c = go.GetComponent<Chunk>();
        c.SetPrefab(prefab);

        go.transform.position = new Vector3(worldX, go.transform.position.y, go.transform.position.z);

        return c;
    }

    private void RecycleChunk(Chunk chunk)
    {
        if (chunk == null) return;
        chunk.gameObject.SetActive(false);
        
        GameObject prefab = chunk.SourcePrefab;
        if (!_pool.ContainsKey(prefab)) _pool[prefab] = new Queue<GameObject>();
        _pool[prefab].Enqueue(chunk.gameObject);
    }
}