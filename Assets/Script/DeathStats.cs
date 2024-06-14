using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathStats : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text survivedWaveText;
    public int totalScore;
    private int totalSurvivedWave;
    private bool isCalculated;

    void Start()
    {
        totalScore = 0;
        totalSurvivedWave = 0;
        isCalculated = false;
    }

    void Update()
    {
        if(Controller.isDead && !isCalculated)
        {
            Debug.Log("Игрок умер! Выводим статистику."); 
            StartCoroutine(getScore());
            StartCoroutine(getSurvivedWave());
            isCalculated = true;
        }
    }

    public IEnumerator getScore()
    {
        for(int i = 0; i < Controller.score; i++)
        {
            yield return new WaitForSeconds(0.005f); 
            totalScore++;
            scoreText.text = totalScore.ToString();
        }
    }

    public IEnumerator getSurvivedWave()
    {
        for(int j = 0; j < WaveManager.currentWave; j++)
        {
            yield return new WaitForSeconds(0.2f); 
            totalSurvivedWave++;
            survivedWaveText.text = totalSurvivedWave.ToString();
        }
    }

    public void ExitMainMenu()
    {
        Controller.score = 0;
        WaveManager.currentWave = 1;
        Controller.isDead = false;
        SceneLoader.currentSceneIndex = 0;
        WeaponManager.currentWeaponIndex = 0;
        SceneManager.LoadSceneAsync("Loading");
    }

    public void RestartGame()
    {
        Controller.score = 0;
        WaveManager.currentWave = 1;
        Controller.isDead = false;
        SceneLoader.currentSceneIndex = 1;
        WeaponManager.currentWeaponIndex = 0;
        SceneManager.LoadSceneAsync("Loading");
    }
}