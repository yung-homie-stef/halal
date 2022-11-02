using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class Drag_Slot : MonoBehaviour, IDropHandler
{
    public string snappedObjectName;
    public static int snappedObjectNum = 0;
    public UnityEvent OnSnappingComplete;
    public UnityEvent OnRitualCardComplete;
    public CanvasGroup canvasGroup;

    public Prayer_Card_Canvas cardCanvasScript;

    public void OnDrop(PointerEventData eventData)
    {
       if (eventData.pointerDrag != null)
       {
            if (eventData.pointerDrag.gameObject.name.Contains(snappedObjectName))
            {
                eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                snappedObjectNum++;
                eventData.pointerDrag.GetComponent<Drag_Drop>().enabled = false;
                canvasGroup.alpha = 1;
                cardCanvasScript.OnMenuItemSelected();

                if (snappedObjectNum == 3)
                {
                    OnSnappingComplete.Invoke();
                }
            }
            
       }
    }

    public void CompleteSnapping()
    {
        Invoke("EndPrayerCardRitual", 5.0f);
    }

    private void EndPrayerCardRitual()
    {
        OnRitualCardComplete.Invoke();
    }
}
