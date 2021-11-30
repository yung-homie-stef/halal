using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Narrator : MonoBehaviour
{
    // singleton
    public static Narrator gameNarrator;

    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    private int _textIndex;
    private float _closeTime = 0.75f;
    private GameObject _chop;
    private Animator _animator;

    private void Awake()
    {
        if (gameNarrator == null)
            gameNarrator = this;
        else if (gameNarrator != this)
            Destroy(gameObject);
    }

    void Start()
    {
        _chop = gameObject.transform.GetChild(0).gameObject;
        _animator = _chop.GetComponent<Animator>();
    }

    void Update()
    {
        if (textComponent.text == lines[_textIndex])
        {
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

    void StartDialogue()
    {
        _textIndex = 0;
        _animator.SetBool("is_talking", true);
        StartCoroutine(TypeOutDialogue());
    }

    private IEnumerator TypeOutDialogue()
    {
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
            _animator.SetBool("is_talking", true);
            _textIndex++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeOutDialogue());
        }
        else
        {
            StartCoroutine(CloseDialogue(_closeTime));
        }
    }

    private IEnumerator CloseDialogue(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        this.gameObject.SetActive(false);
    }
}
