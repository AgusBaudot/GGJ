using UnityEngine;
using UnityEngine.UI;


public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider soundFXVolume;

    void Start()
    {
        CheckPlayerPrefs();
        masterVolume.value = PlayerPrefs.GetFloat("MasterVolume");
        musicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
        soundFXVolume.value = PlayerPrefs.GetFloat("SoundFXVolume");
    }

    public void SetMasterVolumePref() => PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);
    public void SetMusicVolumePref() => PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);
    public void SetSoundFXVolumePref() => PlayerPrefs.SetFloat("SoundFXVolume", soundFXVolume.value);

    private void CheckPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("MasterVolume"))
            PlayerPrefs.SetFloat("MasterVolume", 1);
        
        if (!PlayerPrefs.HasKey("MusicVolume"))
            PlayerPrefs.SetFloat("MusicVolume", 1);
        
        if (!PlayerPrefs.HasKey("SoundFXVolume"))
            PlayerPrefs.SetFloat("SoundFXVolume", 1);
    }
}