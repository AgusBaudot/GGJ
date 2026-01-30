using UnityEngine;

/// <summary>
/// Each mask should define ONLY what changes when worn
/// </summary>

[CreateAssetMenu(fileName = "Mask data", menuName = "SOs/Mask Data")]
public class MaskData : ScriptableObject
{
    [Header("CORE")]
    public int Hp;
    [Tooltip("This number will be multiplied to the base speed.")]
    public float SpeedModifier = 1;
    [Tooltip("This number will be multiplied to the base jump force")]
    public float JumpForceModifier = 1;
    [Tooltip("This number will be multiplied to the base dmg")]
    public float DmgModifier = 1;

    [Header("ATTACK")]
    [Tooltip("Which attack does this mask provide")]
    public AttackType AttackType;

    [Header("OPTIONAL")]
    [Tooltip("Which secondary ability does this mask provide")]
    public SecondaryType SecondaryType;
    [Tooltip("Whether this mask provides double jump or not")]
    public bool DoubleJump;
    
    [Header("VISUALS")]
    public Sprite MaskSprite;
}

public enum AttackType
{
    Basic,
    Ranged,
    Grab
}

public enum SecondaryType
{
    None,
    Dash,
    Teleport
}