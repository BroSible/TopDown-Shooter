using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject playerUI;
    public GameObject settingMenu;
    public GameObject MenuGraphic;
     public GameObject AudioMenu;
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
        _weapons[WeaponManager.currentWeaponIndex].enabled = false;
        Debug.Log("Игра поставлена на паузу.");
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        settingMenu.SetActive(false);
        MenuGraphic.SetActive(false);
        AudioMenu.SetActive(false);

        playerUI.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
        
        StartCoroutine(EnableCameraWithDelay());
        _weaponManager.enabled = true;
        _weapons[WeaponManager.currentWeaponIndex].enabled = true;
        Debug.Log("Игра возобновлена.");
    }

    public void GoToMainMenu()
    {
        //Time.timeScale = 1f;
        Controller.score = 0;
        WaveManager.currentWave = 1;
        Controller.isDead = false;
        SceneLoader.currentSceneIndex = 0;
        WeaponManager.currentWeaponIndex = 0;
        SceneManager.LoadSceneAsync("Loading");
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