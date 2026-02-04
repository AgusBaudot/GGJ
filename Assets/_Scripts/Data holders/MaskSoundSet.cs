using UnityEngine;

[CreateAssetMenu(menuName = "SOs/MaskSoundSet")]
public class MaskSoundSet : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public MaskSoundType Type;
        public SoundFXData Sound;
    }

    public Entry[] Sounds;

    public SoundFXData Get(MaskSoundType type)
    {
        for (int i = 0; i < Sounds.Length; i++)
        {
            if (Sounds[i].Type == type)
                return Sounds[i].Sound;
        }

        return null;
    }
}

public enum MaskSoundType
{
    BasicAttack,
    RangedFireAttack,
    RangedFogAttack,
    GrabAttack
}