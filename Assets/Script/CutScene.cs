using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutScene : Intro
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override IEnumerator wait_for_intro()
    {
        yield return new WaitForSeconds(wait_time);
        SceneLoader.currentSceneIndex = 2;
        SceneManager.LoadSceneAsync("Loading");
    }

    public void SkipButton()
    {
        StopCoroutine(wait_for_intro());
        SceneLoader.currentSceneIndex = 2;
        SceneManager.LoadSceneAsync("Loading");
    }
}
