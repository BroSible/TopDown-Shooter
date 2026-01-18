using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [System.Serializable]
    public class LoadingTip
    {
        [TextArea(2, 4)]
        public string tipText;
        public Texture2D backgroundImage;
        public TipCategory category;
    }

    public enum TipCategory
    {
        Tactical,
        Cooperation,
        Enemies,
        Resources,
        Propaganda,
        Lore,
        Technical
    }

    [Header("UI References")]
    public GameObject loaderUI;
    public Slider progressSlider;
    public Animator transition;
    public RawImage backgroundImage;
    public TextMeshProUGUI tipText;
    public TextMeshProUGUI categoryLabel;
    
    [Header("Custom Progress Bar")]
    public RectTransform rotatingProgressBar;  
    public bool useRotatingProgressBar = true;
    public bool rotateByProgress = true;  
    public float constantRotationSpeed = 180f; 
    public RotationAxis progressBarAxis = RotationAxis.Y;
    
    public enum RotationAxis { X, Y, Z }

    [Header("Scene Settings")]
    public static string[] scenes;
    public static int currentSceneIndex = 0;

    [Header("Tip Settings")]
    public List<LoadingTip> loadingTips = new List<LoadingTip>();
    public float tipChangeInterval = 4f;
    public bool randomizeTips = true;
    public float textFadeSpeed = 2f;

    private int currentTipIndex = 0;
    private Coroutine tipRotationCoroutine;
    private float currentProgressBarRotation = 0f;

    public void Start()
    {
        scenes = new string[] { "Menu", "CutScene", "SampleScene" };
        StartCoroutine(C_LoadScene());
    }

    void Update()
    {
        if (useRotatingProgressBar && !rotateByProgress && rotatingProgressBar != null)
        {
            currentProgressBarRotation += constantRotationSpeed * Time.deltaTime;
            UpdateProgressBarRotation(currentProgressBarRotation);
        }
    }

    public IEnumerator C_LoadScene()
    {
        if (progressSlider != null)
            progressSlider.value = 0;
            
        loaderUI.SetActive(true);
        
        if (transition != null)
            transition.SetTrigger("Start");

        if (randomizeTips && loadingTips.Count > 0)
        {
            currentTipIndex = Random.Range(0, loadingTips.Count);
        }
        
        if (loadingTips.Count > 0)
        {
            DisplayTip(currentTipIndex, true);
        }

        if (loadingTips.Count > 1)
        {
            tipRotationCoroutine = StartCoroutine(RotateTips());
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scenes[currentSceneIndex]);
        asyncOperation.allowSceneActivation = false;
        float progress = 0;

        while (!asyncOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            
            if (progressSlider != null)
                progressSlider.value = progress;
            
            if (useRotatingProgressBar && rotateByProgress)
            {
                float rotationProgress = Mathf.Clamp01(progress / 0.9f);
                UpdateProgressBarRotation(rotationProgress * 360f);
            }
            
            if (progress >= 0.9f)
            {
                if (progressSlider != null)
                    progressSlider.value = 1;
                    
                if (useRotatingProgressBar && rotateByProgress)
                {
                    UpdateProgressBarRotation(360f);
                }
                
                yield return new WaitForSeconds(0.5f);
                asyncOperation.allowSceneActivation = true;
            }
            
            yield return null;
        }

        if (tipRotationCoroutine != null)
        {
            StopCoroutine(tipRotationCoroutine);
        }
    }

    private void UpdateProgressBarRotation(float degrees)
    {
        if (rotatingProgressBar == null) return;
        
        Vector3 rotation = Vector3.zero;
        
        switch (progressBarAxis)
        {
            case RotationAxis.X:
                rotation = new Vector3(degrees, 0, 0);
                break;
            case RotationAxis.Y:
                rotation = new Vector3(0, degrees, 0);
                break;
            case RotationAxis.Z:
                rotation = new Vector3(0, 0, degrees);
                break;
        }
        
        rotatingProgressBar.rotation = Quaternion.Euler(rotation);
    }

    private IEnumerator RotateTips()
    {
        while (true)
        {
            yield return new WaitForSeconds(tipChangeInterval);

            if (randomizeTips)
            {
                int newIndex = Random.Range(0, loadingTips.Count);
                while (newIndex == currentTipIndex && loadingTips.Count > 1)
                {
                    newIndex = Random.Range(0, loadingTips.Count);
                }
                currentTipIndex = newIndex;
            }
            else
            {
                currentTipIndex = (currentTipIndex + 1) % loadingTips.Count;
            }

            yield return StartCoroutine(TransitionToTip(currentTipIndex));
        }
    }

    private IEnumerator TransitionToTip(int tipIndex)
    {
        if (tipText != null)
        {
            Color textColor = tipText.color;
            Color categoryColor = categoryLabel != null ? categoryLabel.color : Color.white;
            
            float alpha = 1f;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime * textFadeSpeed;
                
                textColor.a = alpha;
                tipText.color = textColor;
                
                if (categoryLabel != null)
                {
                    categoryColor.a = alpha;
                    categoryLabel.color = categoryColor;
                }
                
                yield return null;
            }
        }

        DisplayTip(tipIndex, false);

        if (tipText != null)
        {
            Color textColor = tipText.color;
            Color categoryColor = categoryLabel != null ? categoryLabel.color : Color.white;
            
            float alpha = 0f;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * textFadeSpeed;
                
                textColor.a = alpha;
                tipText.color = textColor;
                
                if (categoryLabel != null)
                {
                    categoryColor.a = alpha;
                    categoryLabel.color = categoryColor;
                }
                
                yield return null;
            }
        }
    }

    private void DisplayTip(int index, bool instant = false)
    {
        if (index < 0 || index >= loadingTips.Count) return;

        LoadingTip tip = loadingTips[index];

        if (tipText != null)
        {
            tipText.text = tip.tipText;
            
            if (instant)
            {
                Color textColor = tipText.color;
                textColor.a = 1f;
                tipText.color = textColor;
            }
        }

        if (categoryLabel != null)
        {
            categoryLabel.text = GetCategoryLabel(tip.category);
            
            if (instant)
            {
                Color categoryColor = categoryLabel.color;
                categoryColor.a = 1f;
                categoryLabel.color = categoryColor;
            }
        }

        if (backgroundImage != null && tip.backgroundImage != null)
        {
            backgroundImage.texture = tip.backgroundImage;
        }
    }

    private string GetCategoryLabel(TipCategory category)
    {
        switch (category)
        {
            case TipCategory.Tactical:
                return "ТАКТИКА";
            case TipCategory.Cooperation:
                return "КООПЕРАЦИЯ";
            case TipCategory.Enemies:
                return "ВРАГИ";
            case TipCategory.Resources:
                return "РЕСУРСЫ";
            case TipCategory.Propaganda:
                return "ЗНАЙ СВОЕГО ВРАГА";
            case TipCategory.Lore:
                return "ЛОР";
            case TipCategory.Technical:
                return "ПОДСКАЗКА";
            default:
                return "ИНФОРМАЦИЯ";
        }
    }

    public void AddTip(string text, Texture2D background, TipCategory category)
    {
        loadingTips.Add(new LoadingTip
        {
            tipText = text,
            backgroundImage = background,
            category = category
        });
    }
}