using UnityEngine;
using TMPro;  // Необходимо добавить пространство имён TMPro

public class WaveManager : MonoBehaviour
{
    public GameObject[] beetlePrefab;
    public Transform[] spawnPoints; 
    public static int currentWave = 1;
    public int maxBeetle;
    public int maxAddtoWave = 2;
    public bool isSpawning = false;
    public int beetleSpawned = 0;   
    public float timeToInterval;
    private float interval = 5f;
    public float timeToSpawn;
    [SerializeField] private float timeBetweenSpawn = 5f;

    public enum WaveState
    {
        Active,
        Ended
    }

    public WaveState currentState;

    public TMP_Text waveText;  

    void Update()
    {
        timeToSpawn += Time.deltaTime;

        if (!EnemyIsAlive() && isSpawning && beetleSpawned == maxBeetle)
        {
            currentState = WaveState.Ended;
        }

        switch(currentState)
        {
            case WaveState.Active:
            {
                if(timeToSpawn > timeBetweenSpawn)
                {
                    SpawnEnemy();
                    timeToSpawn = 0;
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
        
        // Добавлено
        Debug.Log($"Текущая волна: {currentWave}");
    }

    public void SpawnEnemy()
    {
        if (beetleSpawned < maxBeetle && spawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPoint = spawnPoints[randomIndex].position;
            int randomIndexPrefab = Random.Range(0, beetlePrefab.Length);
            GameObject newBeetle = Instantiate(beetlePrefab[randomIndexPrefab], spawnPoint, Quaternion.identity);
            newBeetle.SetActive(true);

            beetleSpawned++; 
            isSpawning = true;
        }
    }

    public void SetWaveActive()
    {
        isSpawning = false;
        beetleSpawned = 0;

        if(timeBetweenSpawn != 0.5f)
        {
            timeBetweenSpawn -= 0.25f;
        }

        maxBeetle += maxAddtoWave;
        currentState = WaveState.Active;
        currentWave++;
    }

    bool EnemyIsAlive()
    {
        return GameObject.FindGameObjectWithTag("Enemy") != null;
    }
}