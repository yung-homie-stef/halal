using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Pig_Noises : MonoBehaviour
{
    public AudioClip[] pigNoiseClips;

    [SerializeField]
    private float _soundEffectTimeDelay = 0;
    private AudioSource _source;

    // Start is called before the first frame update
    void Start()
    {
        _source = GetComponent<AudioSource>();
        StartCoroutine(PlayPigNoise(Random.Range(0, _soundEffectTimeDelay)));
    }

    private IEnumerator PlayPigNoise(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

       
        _source.PlayOneShot(pigNoiseClips[Random.Range(0, pigNoiseClips.Length - 1)]);

        StartCoroutine(PlayPigNoise(Random.Range(3, _soundEffectTimeDelay)));
    }

    
}
