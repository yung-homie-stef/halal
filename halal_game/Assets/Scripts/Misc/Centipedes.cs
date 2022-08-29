using System.Collections;
using System.Collections.Generic;
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

    public void UnleashCentipede()
    {
        centipedeAnimators[insectIndex].SetTrigger("crawl");

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
        StartCoroutine(RestartCrawl(index, crawlTimerIntervals[timerIndex]));
    }

    private IEnumerator RestartCrawl(int index, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        if (stillCrawling)
        centipedeAnimators[index].SetTrigger("crawl");
    }
}
