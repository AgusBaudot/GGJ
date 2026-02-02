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

    [Header("Generation Settings")]
    [SerializeField] private float _startOffsetX = 0f; 

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

        float currentGridX = _startOffsetX; // Initialize at the offset
        
        if (_player != null)
        {
             // We add the offset to the player's snapped position
             currentGridX += Mathf.Round(_player.position.x / ChunkSizeX) * ChunkSizeX;
        }

        // Spawn Behind, Current (at offset), and Ahead
        for (int i = -1; i <= 1; i++)
        {
            float x = currentGridX + (i * ChunkSizeX);
            Chunk chunk = GetOrCreateChunk(x);
            _activeChunks.Add(chunk);
        }
    }

    // ... Rest of your OnPlayerEnteredChunk, GetOrCreateChunk, and RecycleChunk logic remains the same ...
    public void OnPlayerEnteredChunk(Chunk enteredChunk)
    {
        if (enteredChunk == null || _activeChunks.Count != 3) return;

        int index = _activeChunks.IndexOf(enteredChunk);
        if (index != 2) return; 

        RecycleChunk(_activeChunks[0]);
        _activeChunks.RemoveAt(0);

        float lastChunkX = _activeChunks[1].transform.position.x;
        float nextX = (Mathf.Round(lastChunkX / ChunkSizeX) * ChunkSizeX) + ChunkSizeX;

        Chunk newAhead = GetOrCreateChunk(nextX);
        _activeChunks.Add(newAhead);

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