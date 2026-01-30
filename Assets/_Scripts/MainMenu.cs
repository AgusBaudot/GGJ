using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OpenGymScene()
    {
        SceneManager.LoadScene("Gym");
    }

    public void OpenOptionsScene()
    {
        SceneManager.LoadScene("Options");
    }

    public void OpenCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
