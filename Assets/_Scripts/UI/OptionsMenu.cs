using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{

    public void OpenAudioAndVideo()
    {
        SceneManager.LoadScene("Audio and Video");
    }

    public void OpenControls()
    {
        SceneManager.LoadScene("Controls");
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
