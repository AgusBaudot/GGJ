using UnityEngine;
using UnityEngine.UI;


public class PlayerSettings : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider soundFXVolume;

    void Start()
    {
        masterVolume.value = PlayerPrefs.GetFloat("MasterVolume");
        musicVolume.value = PlayerPrefs.GetFloat("MusicVolume");
        soundFXVolume.value = PlayerPrefs.GetFloat("SoundFXVolume");
    }

    public void SetMasterVolumePref() => PlayerPrefs.SetFloat("MasterVolume", masterVolume.value);
    public void SetMusicVolumePref() => PlayerPrefs.SetFloat("MusicVolume", musicVolume.value);
    public void SetSoundFXVolumePref() => PlayerPrefs.SetFloat("SoundFXVolume", soundFXVolume.value);

}