using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prayer_Card_Canvas : MonoBehaviour
{
    public AudioClip[] menuSelectClips;
    public AudioClip[] menuClickClips;
    public AudioSource audioSource = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnMenuItemSelected()
    {
        audioSource.PlayOneShot(menuSelectClips[Random.Range(0, menuSelectClips.Length)]);
    }

    public void OnMenuItemClicked()
    {
        audioSource.PlayOneShot(menuClickClips[Random.Range(0, menuClickClips.Length)]);
    }
}
