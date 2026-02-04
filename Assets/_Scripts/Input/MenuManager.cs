using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("AUDIO")]
    [SerializeField] private GlobalSoundsData _globalSounds;
    [Header("MENUS")]
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
                Cursor.visible = true;
                OpenMenu(pauseMenu);
            }
            else
            {
                Cursor.visible = false;
                Back();
            }
        }
    }

    public void OpenMenu(GameObject menu)
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        
        if (currentMenu != null)
        {
            menuStack.Push(currentMenu);
            currentMenu.SetActive(false);
        }

        currentMenu = menu;
        currentMenu.SetActive(true);
        currentMenu.transform.SetAsLastSibling();

        if (PauseManager.Instance != null)
            PauseManager.Instance.SetPauseMenuActive(true);
        else
            Time.timeScale = 0f;

        Cursor.visible = currentMenu != null;
    }

    public void Back()
    {
        SoundFXManager.Instance.Play(_globalSounds.ButtonClick, transform);
        
        if (currentMenu != null)
            currentMenu.SetActive(false);

        currentMenu.SetActive(false);
        currentMenu = null;

        if (PauseManager.Instance != null)
            PauseManager.Instance.SetPauseMenuActive(false);
        else
            Time.timeScale = 1f;

        Cursor.visible = currentMenu != null;
    }
    
    public void OpenOptions() => OpenMenu(optionsMenu);
    public void OpenAudioVideo() => OpenMenu(audioVideoMenu);
    public void OpenConfirmExit() => OpenMenu(confirmExitMenu);
    public void OpenControls() => OpenMenu(controlsMenu);

}