using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    
    public string nextSceneName;

    
    public void LoadScene()
    {
        
        SceneManager.LoadScene("Menu");
    }
}