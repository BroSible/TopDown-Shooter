using UnityEngine;
using TMPro;  // Необходимо добавить пространство имён TMPro

public class WaveManager : MonoBehaviour
{
    public GameObject beetlePrefab;
    public Transform[] spawnPoints; 
    public int currentWave = 1;
    public int maxBeetle;
    public int maxAddtoWave = 2;
    public bool isSpawning = false;
    public int beetleSpawned = 0;   
    public float timeToInterval;
    private float interval = 5f;
    public float timeToStartCoroutine;
    private float timeBetweenStartCoroutine = 5f;

    public enum WaveState
    {
        Active,
        Ended
    }

    public WaveState currentState;

    public TMP_Text waveText;  

    void Update()
    {
        timeToStartCoroutine += Time.deltaTime;

        if (!EnemyIsAlive() && isSpawning && beetleSpawned == maxBeetle)
        {
            currentState = WaveState.Ended;
        }

        switch(currentState)
        {
            case WaveState.Active:
            {
                if(timeToStartCoroutine > timeBetweenStartCoroutine)
                {
                    SpawnEnemy();
                    timeToStartCoroutine = 0;
                }

                break;
            }

            case WaveState.Ended:
            {
                timeToInterval += Time.deltaTime;

                if(timeToInterval > interval)
                {
                    SetWaveActive();
                    timeToInterval = 0;
                }

                break;
            }
        }

        waveText.text = $"Волна: {currentWave}";  
    }

    public void SpawnEnemy()
    {
        if (beetleSpawned < maxBeetle && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPoint = spawnPoints[randomIndex].position;

            GameObject newBeetle = Instantiate(beetlePrefab, spawnPoint, Quaternion.identity);
            newBeetle.SetActive(true);

            beetleSpawned++; 
            isSpawning = true;
        }
    }

    public void SetWaveActive()
    {
        isSpawning = false;
        beetleSpawned = 0;
        maxBeetle += maxAddtoWave;
        currentState = WaveState.Active;
        currentWave++;
    }

    bool EnemyIsAlive()
    {
        return GameObject.FindGameObjectWithTag("Enemy") != null;
    }
}