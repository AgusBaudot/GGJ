using UnityEngine;

/// <summary>
/// EnemyData defines what the enemy is. Behaviour at runtime is handled elsewhere.
/// </summary>

[CreateAssetMenu(fileName = "Enemy data", menuName = "SOs/Enemy data")]
public class EnemyData : ScriptableObject
{
    [Header("ENEMY STATS")]
    public int MaxHP = 5;
    public int ContactDmg = 10;
    public int MoveSpeed = 10;
    public BehaviorType Behavior;
    public MaskData DroppedMask;
    [Header("DETECTION STATS")]
    [Tooltip("Maximum distance at which the enemy will detect player.")]
    public float DetectionRange = 5f;
    [Tooltip("Minimum distance at which the enemy will perform an attack.")]
    public float AttackDistance = 3f;
    [Header("VISUALS")]
    public Sprite EnemySprite;
}

public enum BehaviorType
{
    Hunter, //Fire
    Acrobat, //Movement
    Guardian, //Tank
    Sneaky //Fog
}
