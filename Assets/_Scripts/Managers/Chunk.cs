using UnityEngine;

/// <summary>
/// Attach to chunk prefabs. Requires a Collider2D set as Trigger so the player can trigger
/// chunk entry. Notifies LevelGenerator when the player enters, and stores the source prefab for pooling.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Chunk : MonoBehaviour
{
    /// <summary>
    /// Prefab this instance was created from; used by LevelGenerator for pool return.
    /// </summary>
    public GameObject SourcePrefab { get; private set; }

    public void SetPrefab(GameObject prefab)
    {
        SourcePrefab = prefab;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LevelGenerator.Instance == null) return;
        if (!other.CompareTag("Player")) return;

        LevelGenerator.Instance.OnPlayerEnteredChunk(this);
    }
}
