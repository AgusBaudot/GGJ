using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void Resume()
    {
        SceneManager.LoadScene("Gym");
    }

    public void Restar()
    {
        SceneManager.LoadScene("Gym");
    }

    public void OpenOptionsScene()
    {
        SceneManager.LoadScene("Options");
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("Exit Confirmation");
    }
}
