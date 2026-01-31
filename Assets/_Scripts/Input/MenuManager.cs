using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject audioMenu;

    private GameObject currentMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentMenu == null)
                OpenMenu(pauseMenu);
            else
                CloseCurrentMenu();
        }
    }

    public void OpenMenu(GameObject menu)
    {
        if (currentMenu != null)
            currentMenu.SetActive(false);

        currentMenu = menu;
        currentMenu.SetActive(true);
        currentMenu.transform.SetAsLastSibling();

        Time.timeScale = 0f;
    }

    public void CloseCurrentMenu()
    {
        if (currentMenu == null) return;

        currentMenu.SetActive(false);
        currentMenu = null;

        Time.timeScale = 1f;
    }

    public void OpenOptions()
    {
        OpenMenu(optionsMenu);
    }

    public void OpenAudio()
    {
        OpenMenu(audioMenu);
    }
}
