using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Centipede_Vomit : MonoBehaviour
{
    public AudioSource centipedeAudioSource;
    public ParticleSystem vomitParticle;

    public void PlayCentipedeIdleSound()
    {
        centipedeAudioSource.Play();
    }

    public void Vomit()
    {
        vomitParticle.Play();
    }
}
