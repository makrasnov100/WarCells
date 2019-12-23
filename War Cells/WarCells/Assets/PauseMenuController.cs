using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject PauseMenuComponents;
    public GameObject OptionsMobileButton;
    private bool pauseMenuOn = false;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenuOn = !pauseMenuOn;
            if (pauseMenuOn)
                OptionsManager.Instance.ShowMenu();
            else
                OptionsManager.Instance.HideMenu();
            //PauseMenuComponents.SetActive(pauseMenuOn);
            //OptionsMobileButton.SetActive(pauseMenuOn);
        }
    }

    //NO LONGER USED
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ClosePauseMenu()
    {
        pauseMenuOn = false;
        PauseMenuComponents.SetActive(pauseMenuOn);
        OptionsMobileButton.SetActive(!pauseMenuOn);
    }
    public void OpenPauseMenu()
    {
        pauseMenuOn = true;
        OptionsManager.Instance.ShowMenu();
        //PauseMenuComponents.SetActive(pauseMenuOn);
        //OptionsMobileButton.SetActive(!pauseMenuOn);
    }
}
