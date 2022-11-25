using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMusicFadeIn : MonoBehaviour
{
    [SerializeField]
    public AudioSource musicAudioSource;
    public float fadeInDuration = 0;
    public float fadeOutDuration = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeInDuration, 1));
    }

    public void FadeOutMusic()
    {
        StartCoroutine(AudioSourceFade.StartFade(musicAudioSource, fadeOutDuration, 0));
    }
}
