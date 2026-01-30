using UnityEngine;

/// <summary>
/// Something
/// </summary>

[CreateAssetMenu(menuName = "SOs/MovementDashData", fileName = "New dash data")]
public class MovementDashData : ScriptableObject
{
    [Header("GENERAL")]
    public float Speed;
    public float Duration;
    public float Cooldown;
    public float Buffer;

    [Header("COMBAT")] public bool DealsDamage;
    public float StunDuration;
}