using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Centipedes : MonoBehaviour
{
    [SerializeField]
    private int insectIndex = 0;
    [SerializeField]
    private int timerIndex = 0;
    [SerializeField]
    private float[] crawlTimerIntervals;

    public Animator[] centipedeAnimators;
    private bool stillCrawling = true;

    public GameObject[] centipedes;
    public RandomAudioClipPlayer[] centipedeAudioClipPlayers;
    public bool triggeredVomitYet = false;

    public void UnleashCentipede()
    {
        centipedeAnimators[insectIndex].SetTrigger("crawl");
        centipedeAudioClipPlayers[insectIndex].PlayRandomAudioClip();

        if (insectIndex >= 1)
        {
            timerIndex++;
        }

        insectIndex++;
    }

    public void StopCrawling()
    {
        stillCrawling = false;
    }

    public void InvokeCrawl(int index)
    {
        if (!triggeredVomitYet)
        StartCoroutine(RestartCrawl(index, crawlTimerIntervals[timerIndex]));
        
    }

    private IEnumerator RestartCrawl(int index, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        if (stillCrawling)
        centipedeAnimators[index].SetTrigger("crawl");
        centipedeAudioClipPlayers[index].PlayRandomAudioClip();
        
    }

    public void TriggeredVomit()
    {
        triggeredVomitYet = true;
    }

}
