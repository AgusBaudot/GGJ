using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [Header("AUDIO")]
    [SerializeField] private GlobalSoundsData _globalSounds;
    [Header("PANELS")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _creditsPanel;
    
    public void ToGame()
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        SceneManager.LoadScene("Playing");
    }

    public void ToMainMenuScene()
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            _optionsPanel.SetActive(false);
            _creditsPanel.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }
        else
            SceneManager.LoadScene("Main Menu");
    }

    public void ToOptionsScene()
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        _mainMenuPanel.SetActive(false);
        _optionsPanel.SetActive(true);
    }

    public void ToCreditsScene()
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        _mainMenuPanel.SetActive(false);
        _creditsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        Application.Quit();
    }
}

