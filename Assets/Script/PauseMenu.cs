using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject playerUI;
    public bool isPaused;
    public CameraCursor cameraController = new CameraCursor();
    public BaseWeapon[] _weapons;
    public WeaponManager _weaponManager = new WeaponManager();


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isPaused)
            {
                PauseGame();
            } 

            else
            {
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        playerUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        
        cameraController.enabled = false;
        _weaponManager.enabled = false;
        _weapons[_weaponManager.currentWeaponIndex].enabled = false;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        playerUI.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
        
        StartCoroutine(EnableCameraWithDelay());
        _weaponManager.enabled = true;
        _weapons[_weaponManager.currentWeaponIndex].enabled = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator EnableCameraWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        cameraController.enabled = true;
    }
}

