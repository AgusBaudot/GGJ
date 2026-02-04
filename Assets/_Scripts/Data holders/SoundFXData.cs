using UnityEngine;

[CreateAssetMenu(menuName = "SOs/Sound FX")]
public class SoundFXData : ScriptableObject
{
    public AudioClip[] clips;

    [Range(0f, 1f)] public float Volume = 1f;

    public bool Loop;

    [Range(0.5f, 2f)] public float Pitch = 1f;

    public bool RandomPitch;
    public Vector2 PitchRange = new Vector2(0.9f, 1.1f);
}