using UnityEngine;

/// <summary>
/// Data holder for every ranged attack type.
/// </summary>

[CreateAssetMenu(menuName = "SOs/Ranged attack", fileName = "New ranged attack")]
public class RangedAttackData : ScriptableObject
{
    public int Damage;
    public float Velocity;
    public float LifeTime;
    [Tooltip("Number = how many times the player can shoot per second")]
    public float FireRate;
    
    [Header("VISUALS")]
    public Sprite ProjectileSprite;
}