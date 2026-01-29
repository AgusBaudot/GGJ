using UnityEngine;

/// <summary>
/// Each mask should define ONLY what changes when worn
/// </summary>

[CreateAssetMenu(fileName = "Mask data", menuName = "SOs/Mask Data")]
public class MaskData : ScriptableObject
{
    [Header("Core")]
    public int Hp;
    [Tooltip("This number will be multiplied to the base speed.")]
    public float SpeedModifier = 1;
    [Tooltip("This number will be multiplied to the base jump force")]
    public float JumpForceModifier = 1;
    [Tooltip("This number will be multiplied to the base dmg")]
    public float DmgModifier = 1;
    [Header("Attack")]
    [Tooltip("Does the attack change?")]
    public bool ChangedAttack;
    
    [Header("Optional")]
    public bool HasDash;
    public bool HasDoubleJump;
    public bool HasTeleport;
    
    [Header("Visuals")]
    public Sprite MaskSprite;
}