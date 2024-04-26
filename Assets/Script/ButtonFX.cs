using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFX : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioSource myFx;
    public AudioClip hoverFx;
    public AudioClip clickFx;

    public void HoverSound()
    {
        myFx.PlayOneShot (hoverFx);
    }
    public void ClickSound()
    {
        myFx.PlayOneShot (clickFx);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
