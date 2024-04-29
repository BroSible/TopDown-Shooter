using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour

{
    public GameObject loaderUI;
    public Slider progressSlider;
    public Animator transition;

    public void Start()
    {
        StartCoroutine(C_LoadScene());
    }

    public IEnumerator C_LoadScene()
    {
        progressSlider.value = 0;
        loaderUI.SetActive(true);
        transition.SetTrigger("Start");

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("SampleScene"); //Тут поменять сцену на нужную
        asyncOperation.allowSceneActivation = false;
        float progress = 0;

        while(!asyncOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress,asyncOperation.progress, Time.deltaTime);
            progressSlider.value = progress;
            if(progress >= 0.9f)
            {
                progressSlider.value = 1;
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
