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

    private int _textIndex;
    private float _closeTime = 0.5f;
    private bool _chatting = false;
    private Animator _animator;

    private void Awake()
    {
        if (gameNarrator == null)
            gameNarrator = this;
        else if (gameNarrator != this)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        _animator = chop.GetComponent<Animator>();
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_chatting)
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
        StartCoroutine(TypeOutDialogue());
    }

    private IEnumerator TypeOutDialogue()
    {
        _chatting = true;

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
            _chatting = false;
            Array.Clear(lines, 0, lines.Length);
            lines = new string[0];
            _textIndex = 0;
            StartCoroutine(CloseDialogue(_closeTime));
        }
    }

    private IEnumerator CloseDialogue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        textComponent.text = string.Empty;
        this.gameObject.SetActive(false);
    }
}
