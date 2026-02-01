using UnityEngine;

/// <summary>
/// Attach to spawn location GameObjects under a chunk's "Spawns" child.
/// Defines which enemy types can spawn here. If multiple types are set, one is chosen at random with equal chance.
/// </summary>
public class EnemySpawnPoint : MonoBehaviour
{
    [Tooltip("Enemy types that can spawn at this point. One is chosen at random with equal chance.")]
    public SpawnType[] AllowedTypes = new SpawnType[] { SpawnType.Fire };
}

/// <summary>
/// Matches spawn slots to enemy prefab types (aligned with BehaviorType: Fire=Hunter, Fog=Sneaky, etc.).
/// </summary>
public enum SpawnType
{
    Fire,
    Fog,
    Movement,
    Tank
}
