using UnityEngine;

[CreateAssetMenu(menuName = "SOs/GlobalSoundsData")]
public class GlobalSoundsData : ScriptableObject
{
    [Header("STATES")]
    public SoundFXData Playing;
    public SoundFXData GameOver;
    [Header("PLAYER")]
    public SoundFXData PlayerWalk;
    public SoundFXData PlayerJump;
    public SoundFXData PlayerTeleport;
    public SoundFXData PlayerDash;
    public SoundFXData PlayerEquipMask;
    public SoundFXData PlayerBreakMask;
    public SoundFXData PlayerHit;
    [Header("ENEMIES")]
    public SoundFXData EnemyImpactAfterThrow;
    public SoundFXData EnemyDeath;
    [Header("UI")]
    public SoundFXData ButtonClick;
}
