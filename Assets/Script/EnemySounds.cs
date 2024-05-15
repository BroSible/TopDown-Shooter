using UnityEngine;

public class EnemySounds : MonoBehaviour
{
    // Звуки
    public AudioClip walkSound;
    public AudioClip attackSound;

    // Источник звука
    private AudioSource audioSource;

    // Флаг, указывающий, ходит ли враг
    private bool isWalking = false;

    void Start()
    {
        // Получаем ссылку на источник звука
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Проверяем, ходит ли враг
        if (isWalking)
        {
            // Если источник звука не играет, запускаем звук ходьбы
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkSound;
                audioSource.Play();
            }
        }
        else
        {
            // Если источник звука играет, останавливаем его
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    // Вызывается при запуске анимации ходьбы
    public void OnWalkingStart()
    {
        isWalking = true;
    }

    // Вызывается при завершении анимации ходьбы
    public void OnWalkingEnd()
    {
        isWalking = false;
    }

    // Вызывается при запуске анимации атаки
    public void OnAttackStart()
    {
        // Запускаем звук атаки
        audioSource.PlayOneShot(attackSound);
    }
}
