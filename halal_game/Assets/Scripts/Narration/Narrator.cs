using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Narrator : MonoBehaviour
{
    // singleton
    public static Narrator gameNarrator;

    public Image finishedSentenceIcon;
    public TextMeshProUGUI textComponent;
    public GameObject chop;
    public string[] lines;
    public float textSpeed;
    public Narration_Trigger narrationTriggerObject = null;
    public TMP_FontAsset dyslexicFont = null;
    public TMP_FontAsset linLibertine = null;

    [HideInInspector]
    public bool chatting = false;
    [HideInInspector]
    public bool triggered = false;


    private int _textIndex = 0;
    private float _closeTime = 0.5f;
    
    private Animator _animator = null;
    private GameObject pig_textbox = null;

    private void Awake()
    {
        if (gameNarrator == null)
            gameNarrator = this;
        else if (gameNarrator != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);

        if (PlayerPrefs.GetInt("Dyslexic") == 2)
        {
            textComponent.font = dyslexicFont;
        }
        else
            textComponent.font = linLibertine;
    }

    void Start()
    {
        _animator = chop.GetComponent<Animator>();
        pig_textbox = gameObject.transform.GetChild(0).gameObject;
        pig_textbox.SetActive(false);
    }

    void Update()
    {
        if (chatting)
        #region Either auto-completing lines or going to next line
        {
            if (textComponent.text == lines[_textIndex])
            {
                finishedSentenceIcon.enabled = true;
                _animator.SetBool("is_talking", false);

                if (Input.GetMouseButtonDown(0))
                {
                    NextLine();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StopAllCoroutines();
                    textComponent.text = lines[_textIndex];
                }
            }
        }
        #endregion 
    }

    public void StartDialogue()
    {
        _textIndex = 0;
        _animator.SetBool("is_talking", true);
        triggered = true;
        StartCoroutine(TypeOutDialogue());
    }

    private IEnumerator TypeOutDialogue()
    {
        chatting = true;

        foreach (char c in lines[_textIndex].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (_textIndex < lines.Length - 1)
        {
            finishedSentenceIcon.enabled = false;
            _animator.SetBool("is_talking", true);
            _textIndex++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeOutDialogue());
        }
        else
        {
            chatting = false;
            Array.Clear(lines, 0, lines.Length);
            lines = new string[0];
            _textIndex = 0;
            StartCoroutine(CloseDialogue(_closeTime));
        }
    }

    private IEnumerator CloseDialogue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        narrationTriggerObject.EndOfDialogueEvent();
        textComponent.text = string.Empty;
        pig_textbox.SetActive(false);
        finishedSentenceIcon.enabled = false;
        triggered = false;
    }

    public void UnhideDialogue()
    {
        pig_textbox.SetActive(true);
    }
}
