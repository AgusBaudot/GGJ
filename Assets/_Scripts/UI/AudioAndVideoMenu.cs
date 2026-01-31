using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioAndVideoMenu : MonoBehaviour
{
    public void BackToOptions()
    {
        SceneManager.LoadScene("Options");
    }
}
