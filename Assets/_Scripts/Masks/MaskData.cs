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
    //Attack types? Movesets?
    
    [Header("Optional")]
    public bool HasDash;
    public bool HasShield;
    public bool HasDoubleJump;
    
    [Header("Visuals")]
    public Sprite MaskSprite;
}