using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public Image fadeImage; // Ссылка на Image, который будет использоваться для эффекта
    public float fadeDuration = 1f; // Продолжительность эффекта (в секундах)

    private bool isFading = false; // Флаг, указывающий, происходит ли сейчас эффект
    private float fadeAlpha = 0f; // Текущая прозрачность изображения

    // Метод для запуска эффекта Fade-in
    public void FadeIn()
    {
        if (!isFading)
        {
            isFading = true;
            fadeAlpha = 1f; // Начальное значение прозрачности для Fade-in
        }
    }

    // Метод для запуска эффекта Fade-out
    public void FadeOut()
    {
        if (!isFading)
        {
            isFading = true;
            fadeAlpha = 0f; // Начальное значение прозрачности для Fade-out
        }
    }

    void Update()
    {
        if (isFading)
        {
            // Изменяем прозрачность изображения со временем
            fadeAlpha = Mathf.Lerp(fadeAlpha, (fadeAlpha == 1f) ? 0f : 1f, Time.deltaTime / fadeDuration);
            fadeImage.color = new Color(1f, 1f, 1f, fadeAlpha);

            // Проверяем, завершился ли эффект
            if (Mathf.Approximately(fadeAlpha, (fadeAlpha == 1f) ? 0f : 1f))
            {
                isFading = false;
            }
        }
    }
}