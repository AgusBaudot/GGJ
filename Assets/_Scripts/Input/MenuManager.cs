using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject audioVideoMenu;
    [SerializeField] private GameObject confirmExitMenu;
    [SerializeField] private GameObject controlsMenu;


    private GameObject currentMenu;
    private Stack<GameObject> menuStack = new Stack<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentMenu == null)
            {
                OpenMenu(pauseMenu);
            }
            else
            {
                Back();
            }
        }
    }

    public void OpenMenu(GameObject menu)
    {
        if (currentMenu != null)
        {
            menuStack.Push(currentMenu);
            currentMenu.SetActive(false);
        }

        currentMenu = menu;
        currentMenu.SetActive(true);
        currentMenu.transform.SetAsLastSibling();

        Time.timeScale = 0f;
    }

    public void Back()
    {
        if (currentMenu != null)
            currentMenu.SetActive(false);

        if (menuStack.Count > 0)
        {
            currentMenu = menuStack.Pop();
            currentMenu.SetActive(true);
            currentMenu.transform.SetAsLastSibling();
        }
        else
        {
            currentMenu = null;
            Time.timeScale = 1f;
        }
    }

    // Métodos para botones
    public void OpenOptions() => OpenMenu(optionsMenu);
    public void OpenAudioVideo() => OpenMenu(audioVideoMenu);
    public void OpenConfirmExit() => OpenMenu(confirmExitMenu);
    public void OpenControls() => OpenMenu(controlsMenu);

}
