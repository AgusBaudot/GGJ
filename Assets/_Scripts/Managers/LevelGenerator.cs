using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds the endless world from 15-unit chunk prefabs. Keeps exactly 3 chunks active
/// (behind, current, ahead) and uses object pooling for performance.
/// </summary>
[DefaultExecutionOrder(-100)]
public class LevelGenerator : MonoBehaviour
{
    public const float ChunkSizeX = 15f;

    public static LevelGenerator Instance { get; private set; }

    [SerializeField] private Transform _player;
    [SerializeField] private GameObject[] _chunkPrefabs;

    /// <summary>
    /// Ordered by X: [0] = behind, [1] = current, [2] = ahead.
    /// </summary>
    private readonly List<Chunk> _activeChunks = new List<Chunk>(3);
    private Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_player == null)
            Debug.LogError("LevelGenerator: Assign Player Transform in the Inspector.");
        if (_chunkPrefabs == null || _chunkPrefabs.Length == 0)
            Debug.LogError("LevelGenerator: Assign at least one Chunk prefab.");
    }

    private void Start()
    {
        if (_chunkPrefabs == null || _chunkPrefabs.Length == 0) return;

        float startX = GetStartX();
        for (int i = 0; i < 3; i++)
        {
            float x = startX + i * ChunkSizeX;
            Chunk chunk = GetOrCreateChunk(x);
            _activeChunks.Add(chunk);
        }
    }

    private float GetStartX()
    {
        if (_player != null)
            return Mathf.Floor(_player.position.x / ChunkSizeX) * ChunkSizeX - ChunkSizeX;
        return 0f;
    }

    /// <summary>
    /// Called by Chunk when the player enters its trigger. Advances chunks and recycles the one behind.
    /// </summary>
    public void OnPlayerEnteredChunk(Chunk enteredChunk)
    {
        if (enteredChunk == null || _activeChunks.Count != 3) return;

        int index = _activeChunks.IndexOf(enteredChunk);
        if (index < 0) return;

        // Only advance when player enters the "ahead" chunk (index 2).
        if (index != 2) return;

        RecycleChunk(_activeChunks[0]);
        _activeChunks.RemoveAt(0);

        float nextX = _activeChunks[1].transform.position.x + ChunkSizeX;
        Chunk newAhead = GetOrCreateChunk(nextX);
        _activeChunks.Add(newAhead);
    }

    /// <summary>
    /// Gets an inactive instance from the pool for the given prefab, or instantiates one.
    /// </summary>
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
            if (go.GetComponent<Chunk>() == null)
                go.AddComponent<Chunk>();
        }
        else
            go.SetActive(true);

        Chunk c = go.GetComponent<Chunk>();
        c.SetPrefab(prefab);

        // CHANGE HERE: Add the half-width offset to centered sprites
        float xOffset = ChunkSizeX * 0.5f;
        go.transform.position = new Vector3(worldX + xOffset, go.transform.position.y, go.transform.position.z);

        return c;

        //Chunk c = go.GetComponent<Chunk>();
        //c.SetPrefab(prefab);
        //go.transform.position = new Vector3(worldX, go.transform.position.y, go.transform.position.z);
        //return c;
    }

    private void RecycleChunk(Chunk chunk)
    {
        if (chunk == null) return;

        GameObject prefab = chunk.SourcePrefab;
        if (prefab == null) return;

        chunk.gameObject.SetActive(false);

        if (!_pool.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pool[prefab] = queue;
        }
        queue.Enqueue(chunk.gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
