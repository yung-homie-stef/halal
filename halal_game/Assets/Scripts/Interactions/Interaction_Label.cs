using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class Interaction_Label : MonoBehaviour
{
    // singleton
    public static Interaction_Label globalGameLabelSystem;
    public TextMeshProUGUI letterE;

    private Image _interactionLabelImage = null;

    // Start is called before the first frame update
    void Start()
    {
        _interactionLabelImage = gameObject.GetComponent<Image>();
        _interactionLabelImage.enabled = false;
    }

    private void Awake()
    {
        if (globalGameLabelSystem == null)
            globalGameLabelSystem = this;
        else if (globalGameLabelSystem != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    public void ChangeInteractionSprite(Sprite interactionSprite)
    {
        _interactionLabelImage.enabled = true;
        _interactionLabelImage.sprite = interactionSprite;
        letterE.enabled = true;
    }

    public void HideInteractionSprite()
    {
        _interactionLabelImage.enabled = false;
        letterE.enabled = false;
    }
}
