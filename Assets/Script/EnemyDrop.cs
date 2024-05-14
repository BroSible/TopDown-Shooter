using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDrop : MonoBehaviour
{
    public GameObject[] bonusPrefabs; // Массив префабов бонусов
    public float dropChance; // Шанс выпадения бонуса (от 0 до 1)

    void Start()
    {
        // Получить компонент здоровья врага
        Health health = GetComponent<Health>();

        // Подписаться на событие смерти врага
        health.OnDie += OnEnemyDeath;
    }

    private void OnEnemyDeath()
    {
        // Проверить шанс выпадения бонуса
        if (Random.value < dropChance)
        {
            // Выбрать случайный бонус
            int randomIndex = Random.Range(0, bonusPrefabs.Length);

            // Создать бонус в позиции и с вращением врага
            GameObject bonus = Instantiate(bonusPrefabs[randomIndex], transform.position, transform.rotation);

        }
    }
}

internal class Health
{
    public System.Action OnDie { get; internal set; }
}