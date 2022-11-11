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

    [Header("Text")]
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

    [SerializeField]
    private int characterCount = 0;

    private int _textIndex = 0;
    private float _closeTime = 0.5f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip[] talkingSounds;
    private AudioSource audioSource;
    [SerializeField]
    private bool _stopAudioSource = true;
    public int dialogueSoundFrequency = 3;
    [SerializeField]
    [Range(-3, 3)]
    private float _minimumPitch;
    [Range(-3, 3)]
    [SerializeField]
    private float _maximumPitch = 3.0f;
    public bool makePredictable = false;


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

        audioSource = gameObject.GetComponent<AudioSource>();

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
            if (textComponent.maxVisibleCharacters >= characterCount)
            {
                finishedSentenceIcon.enabled = true;
                _animator.SetBool("is_talking", false);

                if (Input.GetMouseButtonDown(0))
                {
                    if (!Global_Settings_Manager.instance.isPaused)
                    {
                        NextLine();
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!Global_Settings_Manager.instance.isPaused)
                    {
                        StopAllCoroutines();
                        textComponent.maxVisibleCharacters = characterCount;
                    }
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
        textComponent.text = lines[_textIndex];
        textComponent.maxVisibleCharacters = 0;

        characterCount = lines[_textIndex].Length;

        for (int i = 0; textComponent.maxVisibleCharacters < characterCount; i++)
        {
            PlayDialogueSound(textComponent.maxVisibleCharacters, textComponent.text[textComponent.maxVisibleCharacters]);
            textComponent.maxVisibleCharacters++;
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

    private void PlayDialogueSound(int displayedCharacterCount, char currentCharacter)
    {
        if (displayedCharacterCount % dialogueSoundFrequency == 0)
        {

            if (_stopAudioSource)
            {
                audioSource.Stop();
            }

            AudioClip randomSoundClip = null;

            if (makePredictable)
            {
                int hashCode = currentCharacter.GetHashCode();
                int predictableIndex = hashCode % talkingSounds.Length;
                randomSoundClip = talkingSounds[predictableIndex];
                int minimumPitchInteger = (int)(_minimumPitch * 100);
                int maximumPitchInteger = (int)(_maximumPitch * 100);
                int pitchRangeInt = maximumPitchInteger - minimumPitchInteger;

                if (pitchRangeInt != 0)
                {
                    int predictablePitchInt = (hashCode % pitchRangeInt) + minimumPitchInteger;
                    float predictablePitch = predictablePitchInt / 100f;
                    audioSource.pitch = predictablePitchInt;
                }
                else
                    audioSource.pitch = _minimumPitch;

            }
            else
            {
                randomSoundClip = talkingSounds[UnityEngine.Random.Range(0, talkingSounds.Length)];
                audioSource.pitch = UnityEngine.Random.Range(_minimumPitch, _maximumPitch);
            }

           
            audioSource.PlayOneShot(randomSoundClip);
        }
    }
}
