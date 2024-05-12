using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] musicClips;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomClip();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            PlayRandomClip();
        }
    }

    private void PlayRandomClip()
    {
        int randomIndex = UnityEngine.Random.Range(0, musicClips.Length);
        audioSource.clip = musicClips[randomIndex];
        audioSource.Play();
    }
}
