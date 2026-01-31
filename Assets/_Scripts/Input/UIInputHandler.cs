using UnityEngine;
using UnityEngine.SceneManagement;

public class UIInputHandler : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}