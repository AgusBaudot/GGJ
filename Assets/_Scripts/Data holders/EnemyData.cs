using UnityEngine;

/// <summary>
/// EnemyData defines what the enemy is. Behaviour at runtime is handled elsewhere.
/// </summary>

[CreateAssetMenu(fileName = "Enemy data", menuName = "SOs/Enemy data")]
public class EnemyData : ScriptableObject
{
    public int MaxHP = 5;
    public int ContactDmg = 10;
    public int MoveSpeed = 10;
    public BehaviorType Behavior;
    public MaskData DroppedMask;
    public Sprite EnemySprite;
}

public enum BehaviorType
{
    Walker,
    Charger,
    Jumper,
    Ranged
} //Idk what else
