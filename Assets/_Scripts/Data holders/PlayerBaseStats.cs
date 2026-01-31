using UnityEngine;

/// <summary>
/// Base stats for true form.
/// </summary>

[CreateAssetMenu(menuName = "SOs/Player stats", fileName = "New player stats")]
public class PlayerBaseStats : ScriptableObject
{
    [Header("LAYERS")]
    [Tooltip("The layer the player is on.")] public LayerMask PlayerLayer;
    [Tooltip("The layers the game will interpret as ground.")] public LayerMask GroundLayers;

    [Header("INPUT")]
    [Tooltip(
        "Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keybaord parity.")]
    public bool SnapInput = true;

    [Tooltip(
         "Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"),
     Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f;

    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"),
     Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;

    [Header("MOVEMENT")] [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;

    [Tooltip("The player's capacity to gain horizontal speed")]
    public float Acceleration = 120;

    [Tooltip("The pace at which the player comes to a stop")]
    public float GroundDeceleration = 60;

    [Tooltip("Deceleration in air only after stopping input mid-air")]
    public float AirDeceleration = 30;

    [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
    public float GroundingForce = -1.5f;

    [Tooltip("The detection distance for grounding and roof detection"), Range(0f, 0.5f)]
    public float GrounderDistance = 0.05f;

    [Header("JUMP")] [Tooltip("The immediate velocity applied when jumping")]
    public float JumpPower = 36;

    [Tooltip("The maximum vertical movement speed")]
    public float MaxFallSpeed = 40;

    [Tooltip("The player's capacity to gain fall speed. a.k.a. In Air Gravity")]
    public float FallAcceleration = 110;

    [Tooltip("The gravity multiplier added when jump is released early")]
    public float JumpEndEarlyGravityModifier = 3;

    [Tooltip(
        "The time before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
    public float CoyoteTime = .15f;

    [Tooltip("The amount of time we buffer a jump. This allows jump input before actually hitting the ground")]
    public float JumpBuffer = .2f;

    [Header("DASHING")]
    public MovementDashData DashData;
    
    [Tooltip("The velocity multiplier multiplied when player is dashing")]
    public float DashSpeed = 2f;

    [Tooltip("The amount of time the dash lasts")]
    public float DashDuration = 0.75f;

    [Tooltip("The amount of time the player has to wait before using the dash again")]
    public float DashCooldown = 1.5f;

    [Tooltip("The amount of time we buffer a dash. This allows dash input before actually having a dash available")]
    public float DashBuffer = .2f;
    
    [Header("TELEPORT")]
    [Tooltip("Distance in units that the player will be teleported")]
    public float TeleportDistance = 3f;
    
    [Tooltip("The amount of time the player dissapears before re-appearing post-teleport")]
    public float TeleportDuration = .2f;
    
    [Tooltip("The amount of time the player has to wait before using the teleport again")]
    public float TeleportCooldown = 3f;
    
    [Tooltip("The amount of time we buffer a teleport. This allows teleport input before actually having a teleport available")]
    public float  TeleportBuffer = .2f;

    [Tooltip("The amount of time the dissolve animation takes.")]
    public float TeleportDissolveDuration = 0.2f;

    [Tooltip("The amount of time the reappear animation takes.")]
    public float TeleportReappearDuration = 0.2f;

    [Header("BASIC ATTACK")]
    public MovementDashData TackleData;
    
    [Tooltip("The amount of damage the player inflicts in its true form.")]
    public float BaseDamage = 5;

    [Tooltip("The amount of speed the player has during the basic attack.")]
    public float BasicAttackSpeed = 15f;
    
    [Tooltip("The amount of time the attack lasts for.")]
    public float BasicAttackDuration = 2f;

    [Tooltip("The amount of time the player has to wait before doing another attack.")]
    public float BasicAttackCooldown = 1f;
    
    [Tooltip("The amount of time the enemy is stunned if the player connects this attack.")]
    public float BasicAttackStunDuration = 0.5f;

    [Tooltip("The amount of time we buffer the attack. This allows the attack input before actually having an attack available.")]
    public float BasicAttackBuffer = 0.2f;

    [Header("GRAB ATTACK")] //Damage dealt by this attack is located in its mask.
    public float GrabAttackRange = 1f;

    [Tooltip("Force magnitude for voluntary throw")]
    public float ThrowForce = 20f;

    [Tooltip("Force magnitude for damage-triggered (weaker) throw")]
    public float DropForce = 10f;
    
    [Tooltip("Direction for voluntary throw (e.g. (1, 1) for up-forward); normalized when applying")]
    public Vector2 ThrowDirection = Vector2.one;

    [Tooltip("Direction for damage-triggered auto-throw")]
    public Vector2 DropDirection = new Vector2(0.5f, 0.5f);

    [Tooltip("The amount of time the player has to wait before being able to perform another grab attack again. Cooldown will start when enemy is thrown.")]
    public float GrabAttackCooldown = 2f;

    [Tooltip("The amount time we buffer this attack. This allows the attack input before actually having an attack available.")]
    public float GrabAttackBuffer = 0.2f;
}