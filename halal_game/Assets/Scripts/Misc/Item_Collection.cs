using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Collection : MonoBehaviour
{
    public GameObject item;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // play pickup sound
            item.SetActive(true);
            Destroy(this.gameObject);
        }
    }
}
