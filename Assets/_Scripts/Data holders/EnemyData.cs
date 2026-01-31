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
    public float DetectionDistance = 5f;
    [Tooltip("Minimum distance at which the enemy will perform an attack. Ranged enemies should have this set to DetectionDistance.")]
    public float AttackDistance = 3f;
    [Tooltip("At this distance the enemy will escape from the player. 0 means no flee, aggressive behavior.")]
    public float FleeDistance = 0f;
    
    [Header("ACROBAT SPECIFIC")]
    [Tooltip("Jump force applied when hopping in place (idle)")]
    public float JumpForce = 10f;
    [Tooltip("Horizontal speed when approaching player")]
    public float ApproachHorizontalSpeed = 5f;
    [Tooltip("Vertical jump force when approaching player")]
    public float ApproachJumpForce = 10f;
    [Tooltip("Force applied during dash attack")]
    public float DashSpeed = 20f;
    [Tooltip("Duration of the dash attack")]
    public float DashDuration = 0.3f;
    [Tooltip("Cooldown between attack decisions")]
    public float AttackCooldown = 2f;
    
    [Header("HUNTER SPECIFIC")]
    [Tooltip("Speed when walking backwards to flee")]
    public float BackwardsSpeed = 3f;
    [Tooltip("Projectile prefab to spawn")]
    public GameObject ProjectilePrefab;
    [Tooltip("Data for the projectile stats")]
    public RangedAttackData RangedAttackData;
    
    [Header("GUARDIAN SPECIFIC")]
    [Tooltip("Time to wait before executing grab (telegraph)")]
    public float GrabWindupTime = 1f;
    [Tooltip("Offset from enemy position for grab hitbox")]
    public Vector2 GrabOffset = new Vector2(1f, 0.5f);
    [Tooltip("Size of the grab hitbox")]
    public Vector2 GrabSize = new Vector2(1f, 2f);
    [Tooltip("How long to hold player before throwing")]
    public float PlayerHoldDuration = 0.5f;
    [Tooltip("Offset where player is held above enemy")]
    public Vector2 PlayerHoldOffset = new Vector2(0f, 1.5f);
    [Tooltip("Direction for throw (e.g. (1, 1) for up-forward); normalized when applying. X is multiplied by facing.")]
    public Vector2 ThrowDirection = new Vector2(1f, 1f);
    [Tooltip("Force applied when throwing player (impulse, like player grab throw)")]
    public float ThrowForce = 15f;
    [Tooltip("Cooldown after missing a grab")]
    public float GrabMissCooldown = 1f;
    
    [Header("SNEAKY SPECIFIC")]
    [Tooltip("Distance to teleport away from player")]
    public float TeleportDistance = 5f;
    [Tooltip("Cooldown between teleports")]
    public float TeleportCooldown = 3f;
    [Tooltip("Visual dissolve duration before teleport (optional)")]
    public float TeleportDissolveDuration = 0f;
    [Tooltip("Visual reappear duration after teleport (optional)")]
    public float TeleportReappearDuration = 0f;
    
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