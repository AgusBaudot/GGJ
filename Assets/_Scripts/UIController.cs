using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public void ToGymScene()
    {
        SceneManager.LoadScene("Gym");
    }

    public void ToMainMenuScene()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void ToOptionsScene()
    {
        SceneManager.LoadScene("Options");
    }

    public void ToCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }

    public void ToControlsScene()
    {
        SceneManager.LoadScene("Controls");
    }

    public void ToAudioAndVideoScene()
    {
        SceneManager.LoadScene("Audio and Video");
    }

    public void Pause()
    {
        SceneManager.LoadScene("Pause");
    }

    public void Results()
    {
        SceneManager.LoadScene("Results");
    }

    public void Defeat()
    {
        SceneManager.LoadScene("Defeat");
    }

    public void ExitConfirmation()
    {
        SceneManager.LoadScene("Exit Confirmation");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

