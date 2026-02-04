using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] private AudioSource _soundFXPrefab;
    
    //Track looping sounds per owner
    private Dictionary<Object, AudioSource> _loopingSources = new();

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Plays one shot sounds
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="spawnTransform"></param>
    public void Play(SoundFXData sound, Transform spawnTransform)
    {
        if (sound == null || sound.clips.Length == 0)
            return;

        AudioSource source = Instantiate(_soundFXPrefab, spawnTransform.position, Quaternion.identity);

        ConfigureSource(source, sound);
        source.Play();
        
        if (!sound.Loop)
            Destroy(source.gameObject, source.clip.length);
    }
    
    //Looping start.
    public void PlayLoop(SoundFXData sound, Transform owner)
    {
        if (_loopingSources.ContainsKey(owner))
            return;

        AudioSource source = Instantiate(_soundFXPrefab, owner.position, Quaternion.identity);

        ConfigureSource(source, sound);
        source.loop = true;
        source.Play();
        
        _loopingSources.Add(owner, source);
    }
    
    //Looping stop.
    public void StopLoop(Transform owner)
    {
        if (!_loopingSources.TryGetValue(owner, out AudioSource source))
            return;
        
        Destroy(source.gameObject);
        _loopingSources.Remove(owner);
    }

    private void ConfigureSource(AudioSource source, SoundFXData sound)
    {
        source.clip = sound.clips[Random.Range(0, sound.clips.Length)];
        source.volume = sound.Volume;

        source.pitch = sound.RandomPitch
            ? Random.Range(sound.PitchRange.x, sound.PitchRange.y)
            : sound.Pitch;
    }
}