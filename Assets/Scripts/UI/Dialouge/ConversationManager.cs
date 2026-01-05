using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConversationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private Image speakerImage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject conversationPanel;

    [Header("Settings")]
    [SerializeField] private int maxCharactersPerSegment = 200;
    [SerializeField] private bool autoPlayVoice = true;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool useTypewriter = false;

    [Header("Text Segmentation")]
    [SerializeField] private float minBreakPointRatio = 0.5f;
    [Tooltip("Minimum ratio of max length to use sentence break (0.5 = 50%)")]

    [Header("Events")]
    public UnityEvent OnConversationStart;
    public UnityEvent OnConversationEnd;
    public UnityEvent<int, int> OnSegmentChange;

    // State
    private Conversation currentConversation;
    private int currentLineIndex;
    private int currentSegmentIndex;
    private int totalSegments;
    private bool isTyping;
    private Coroutine typewriterCoroutine;

    // Cache
    private DialogueLine cachedCurrentLine;
    private ThirdPersonCameraController cameraController;

    // Constants
    private const int INITIAL_LINE_INDEX = 0;
    private const int INITIAL_SEGMENT_INDEX = 0;
    private const int INITIAL_TOTAL_SEGMENTS = 0;

    void Awake()
    {
        CacheComponents();
    }

    void Start()
    {
        InitializeConversationPanel();
        RegisterEventListeners();
    }

    private void CacheComponents()
    {
        if (Camera.main != null)
        {
            cameraController = Camera.main.GetComponent<ThirdPersonCameraController>();
        }
    }

    private void InitializeConversationPanel()
    {
        if (conversationPanel != null)
        {
            conversationPanel.SetActive(false);
        }
    }

    private void RegisterEventListeners()
    {
        OnConversationStart.AddListener(DisableCamera);
        OnConversationEnd.AddListener(EnableCamera);
    }

    public void StartConversation(Conversation conversation)
    {
        if (!IsValidConversation(conversation))
        {
            Debug.LogWarning("Cannot start empty or null conversation!");
            return;
        }

        InitializeConversation(conversation);
        ProcessConversation();
        ShowConversationPanel();

        OnConversationStart?.Invoke();
        DisplayCurrentSegment();
    }

    private bool IsValidConversation(Conversation conversation)
    {
        return conversation != null &&
               conversation.dialogueLines != null &&
               conversation.dialogueLines.Count > 0;
    }

    private void InitializeConversation(Conversation conversation)
    {
        currentConversation = conversation;
        currentLineIndex = INITIAL_LINE_INDEX;
        currentSegmentIndex = INITIAL_SEGMENT_INDEX;
        cachedCurrentLine = null;
    }

    private void ShowConversationPanel()
    {
        if (conversationPanel != null)
        {
            conversationPanel.SetActive(true);
        }
    }

    private void ProcessConversation()
    {
        totalSegments = INITIAL_TOTAL_SEGMENTS;

        foreach (var line in currentConversation.dialogueLines)
        {
            ProcessDialogueLine(line);
        }
    }

    private void ProcessDialogueLine(DialogueLine line)
    {
        line.textSegments.Clear();

        if (string.IsNullOrEmpty(line.dialogueText))
        {
            AddEmptySegment(line);
            return;
        }

        SegmentText(line);
    }

    private void AddEmptySegment(DialogueLine line)
    {
        line.textSegments.Add(string.Empty);
        totalSegments++;
    }

    private void SegmentText(DialogueLine line)
    {
        string remainingText = line.dialogueText;

        while (remainingText.Length > 0)
        {
            if (remainingText.Length <= maxCharactersPerSegment)
            {
                AddFinalSegment(line, remainingText);
                break;
            }

            int breakPoint = FindBreakPoint(remainingText, maxCharactersPerSegment);
            string segment = remainingText.Substring(0, breakPoint).Trim();

            line.textSegments.Add(segment);
            totalSegments++;

            remainingText = remainingText.Substring(breakPoint).Trim();
        }
    }

    private void AddFinalSegment(DialogueLine line, string text)
    {
        line.textSegments.Add(text);
        totalSegments++;
    }

    private int FindBreakPoint(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text.Length;
        }

        int sentenceBreak = FindSentenceBreak(text, maxLength);
        if (IsValidBreakPoint(sentenceBreak, maxLength))
        {
            return sentenceBreak + 1;
        }

        int spaceBreak = text.LastIndexOf(' ', maxLength);
        if (spaceBreak > 0)
        {
            return spaceBreak;
        }

        return maxLength;
    }

    private int FindSentenceBreak(string text, int maxLength)
    {
        int lastPeriod = text.LastIndexOf('.', maxLength);
        int lastExclamation = text.LastIndexOf('!', maxLength);
        int lastQuestion = text.LastIndexOf('?', maxLength);

        return Mathf.Max(lastPeriod, lastExclamation, lastQuestion);
    }

    private bool IsValidBreakPoint(int breakPoint, int maxLength)
    {
        return breakPoint > maxLength * minBreakPointRatio;
    }

    private void DisplayCurrentSegment()
    {
        if (currentConversation == null)
        {
            return;
        }

        UpdateCachedLine();
        string textToDisplay = GetCurrentSegmentText();

        UpdateSpeakerVisual();
        PlayVoiceIfNeeded();
        DisplayText(textToDisplay);
        NotifySegmentChange();
    }

    private void UpdateCachedLine()
    {
        cachedCurrentLine = currentConversation.dialogueLines[currentLineIndex];
    }

    private string GetCurrentSegmentText()
    {
        return cachedCurrentLine.textSegments[currentSegmentIndex];
    }

    private void UpdateSpeakerVisual()
    {
        if (speakerImage == null)
        {
            return;
        }

        if (cachedCurrentLine.speakerSprite != null)
        {
            speakerImage.sprite = cachedCurrentLine.speakerSprite;
            speakerImage.enabled = true;
        }
        else
        {
            speakerImage.enabled = false;
        }
    }

    private void PlayVoiceIfNeeded()
    {
        if (!ShouldPlayVoice())
        {
            return;
        }

        audioSource.PlayOneShot(cachedCurrentLine.voiceClip);
    }

    private bool ShouldPlayVoice()
    {
        return currentSegmentIndex == 0 &&
               autoPlayVoice &&
               cachedCurrentLine.voiceClip != null &&
               audioSource != null;
    }

    private void DisplayText(string text)
    {
        if (dialogueSystem == null)
        {
            return;
        }

        if (useTypewriter)
        {
            StartTypewriterEffect(text);
        }
        else
        {
            dialogueSystem.DisplayDialogue(text);
        }
    }

    private void StartTypewriterEffect(string text)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
    }

    private void NotifySegmentChange()
    {
        int absoluteSegmentIndex = GetAbsoluteSegmentIndex();
        OnSegmentChange?.Invoke(absoluteSegmentIndex + 1, totalSegments);
    }

    private System.Collections.IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        System.Text.StringBuilder displayedText = new System.Text.StringBuilder(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            displayedText.Append(text[i]);

            if (dialogueSystem != null)
            {
                dialogueSystem.DisplayDialogue(displayedText.ToString());
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    public void NextSegment()
    {
        if (!CanAdvance())
        {
            return;
        }

        if (HasMoreSegmentsInCurrentLine())
        {
            AdvanceToNextSegment();
        }
        else if (HasMoreLines())
        {
            AdvanceToNextLine();
        }
        else
        {
            EndConversation();
        }
    }

    private bool CanAdvance()
    {
        return currentConversation != null && !isTyping;
    }

    private bool HasMoreSegmentsInCurrentLine()
    {
        DialogueLine currentLine = GetCurrentOrCachedLine();
        return currentSegmentIndex < currentLine.textSegments.Count - 1;
    }

    private bool HasMoreLines()
    {
        return currentLineIndex < currentConversation.dialogueLines.Count - 1;
    }

    private DialogueLine GetCurrentOrCachedLine()
    {
        return cachedCurrentLine ?? currentConversation.dialogueLines[currentLineIndex];
    }

    private void AdvanceToNextSegment()
    {
        currentSegmentIndex++;
        DisplayCurrentSegment();
    }

    private void AdvanceToNextLine()
    {
        currentLineIndex++;
        currentSegmentIndex = INITIAL_SEGMENT_INDEX;
        cachedCurrentLine = null;
        DisplayCurrentSegment();
    }

    public void PreviousSegment()
    {
        if (!CanGoBack())
        {
            return;
        }

        if (currentSegmentIndex > 0)
        {
            GoToPreviousSegment();
        }
        else if (currentLineIndex > 0)
        {
            GoToPreviousLine();
        }
    }

    private bool CanGoBack()
    {
        return currentConversation != null && !isTyping;
    }

    private void GoToPreviousSegment()
    {
        currentSegmentIndex--;
        DisplayCurrentSegment();
    }

    private void GoToPreviousLine()
    {
        currentLineIndex--;
        cachedCurrentLine = currentConversation.dialogueLines[currentLineIndex];
        currentSegmentIndex = cachedCurrentLine.textSegments.Count - 1;
        DisplayCurrentSegment();
    }

    private int GetAbsoluteSegmentIndex()
    {
        int index = 0;

        for (int i = 0; i < currentLineIndex; i++)
        {
            index += currentConversation.dialogueLines[i].textSegments.Count;
        }

        return index + currentSegmentIndex;
    }

    public void EndConversation()
    {
        CleanupConversation();
        HideConversationPanel();
        ResetConversationState();

        OnConversationEnd?.Invoke();
    }

    private void CleanupConversation()
    {
        StopTypewriterIfRunning();
        StopAudioIfPlaying();
    }

    private void StopTypewriterIfRunning()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
    }

    private void StopAudioIfPlaying()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void HideConversationPanel()
    {
        if (conversationPanel != null)
        {
            conversationPanel.SetActive(false);
        }
    }

    private void ResetConversationState()
    {
        currentConversation = null;
        cachedCurrentLine = null;
        currentLineIndex = INITIAL_LINE_INDEX;
        currentSegmentIndex = INITIAL_SEGMENT_INDEX;
        isTyping = false;
    }

    public void SkipTypewriter()
    {
        if (!IsTypewriterActive())
        {
            return;
        }

        StopTypewriterIfRunning();
        isTyping = false;

        DisplayFullSegmentText();
    }

    private bool IsTypewriterActive()
    {
        return isTyping && typewriterCoroutine != null;
    }

    private void DisplayFullSegmentText()
    {
        if (dialogueSystem == null || cachedCurrentLine == null)
        {
            return;
        }

        string fullText = cachedCurrentLine.textSegments[currentSegmentIndex];
        dialogueSystem.DisplayDialogue(fullText);
    }

    public void PlayCurrentVoice()
    {
        if (!CanPlayVoice())
        {
            return;
        }

        audioSource.PlayOneShot(cachedCurrentLine.voiceClip);
    }

    private bool CanPlayVoice()
    {
        return currentConversation != null &&
               cachedCurrentLine != null &&
               cachedCurrentLine.voiceClip != null &&
               audioSource != null;
    }

    private void DisableCamera()
    {
        if (cameraController == null)
        {
            return;
        }

        cameraController.SetCameraRotationEnabled(false);
        FreezePlayerVerticalMovement();
    }

    private void EnableCamera()
    {
        if (cameraController == null)
        {
            return;
        }

        cameraController.SetCameraRotationEnabled(true);
        UnfreezePlayerMovement();
    }

    private void FreezePlayerVerticalMovement()
    {
        if (cameraController.target == null)
        {
            return;
        }

        Rigidbody rb = cameraController.target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY;
        }
    }

    private void UnfreezePlayerMovement()
    {
        if (cameraController.target == null)
        {
            return;
        }

        Rigidbody rb = cameraController.target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}
