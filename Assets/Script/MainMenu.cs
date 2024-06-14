using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneLoader.currentSceneIndex = 1;
        SceneManager.LoadSceneAsync("Loading");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
