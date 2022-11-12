using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioClipPlayer : MonoBehaviour
{
    public AudioClip[] audioClips;
    public bool usesPitch = false;

    public float minimumPitch = 0.0f;
    public float maximumPitch = 0.0f;

    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandomAudioClip()
    {
        if (usesPitch)
        {
            _audioSource.pitch = Random.Range(minimumPitch, maximumPitch);
        }
        _audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
    }
}
