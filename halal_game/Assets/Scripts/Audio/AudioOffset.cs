using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class AudioOffset : MonoBehaviour
{
    private AudioSource _source = null;

    // Start is called before the first frame update
    void Start()
    {
        _source = GetComponent<AudioSource>();
        _source.time = Random.Range(0f, _source.clip.length);
    }

}
