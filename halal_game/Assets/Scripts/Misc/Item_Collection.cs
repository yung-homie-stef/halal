using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Collection : MonoBehaviour
{
    public GameObject item;

    public void CollectItem()
    {
            // play pickup sound
            item.SetActive(true);
            Destroy(this.gameObject);
        
    }
}
