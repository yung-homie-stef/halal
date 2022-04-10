using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill_Count : MonoBehaviour
{
    static public int pigCount = 3;
    public bool isDead = false;

    public Narration_ComponentEnabler gunTextBoxes = null;


    public void TallyPigKill()
    {
        if (!isDead)
        {
            pigCount--;
            gunTextBoxes.gameObject.SetActive(true);
            Debug.Log(pigCount);

            if (pigCount == 0)
            {
                
                // trigger going to the centipede level
            }

            isDead = true;
        }
    }
}
