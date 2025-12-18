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

    [Header("Input Settings")]
    [Tooltip("Connect your Input System actions here")]
    public bool enableInputNavigation = true;

    [Header("Settings")]
    [SerializeField] private int maxCharactersPerSegment = 200;
    [SerializeField] private bool autoPlayVoice = true;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool useTypewriter = false;

    [Header("Events")]
    public UnityEvent OnConversationStart;
    public UnityEvent OnConversationEnd;
    public UnityEvent<int, int> OnSegmentChange; // current segment, total segments

    private Conversation currentConversation;
    private int currentLineIndex = 0;
    private int currentSegmentIndex = 0;
    private int totalSegments = 0;

    private bool isTyping = false;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        if (conversationPanel != null)
            conversationPanel.SetActive(false);
        OnConversationStart.AddListener(() => DisableCamera());
        OnConversationEnd.AddListener(() => EnableCamera());
    }

    public void StartConversation(Conversation conversation)
    {
        if (conversation == null || conversation.dialogueLines.Count == 0)
        {
            Debug.LogWarning("Cannot start empty conversation!");
            return;
        }

        currentConversation = conversation;
        ProcessConversation();

        currentLineIndex = 0;
        currentSegmentIndex = 0;

        if (conversationPanel != null)
            conversationPanel.SetActive(true);

        OnConversationStart?.Invoke();
        DisplayCurrentSegment();
    }

    private void ProcessConversation()
    {
        totalSegments = 0;

        foreach (var line in currentConversation.dialogueLines)
        {
            line.textSegments.Clear();

            if (string.IsNullOrEmpty(line.dialogueText))
            {
                line.textSegments.Add("");
                totalSegments++;
                continue;
            }

            // Split text into segments that fit the character limit
            string remainingText = line.dialogueText;

            while (remainingText.Length > 0)
            {
                if (remainingText.Length <= maxCharactersPerSegment)
                {
                    line.textSegments.Add(remainingText);
                    totalSegments++;
                    break;
                }

                // Find a good breaking point (space, punctuation)
                int breakPoint = FindBreakPoint(remainingText, maxCharactersPerSegment);

                string segment = remainingText.Substring(0, breakPoint).Trim();
                line.textSegments.Add(segment);
                totalSegments++;

                remainingText = remainingText.Substring(breakPoint).Trim();
            }
        }
    }

    private int FindBreakPoint(string text, int maxLength)
    {
        if (text.Length <= maxLength)
            return text.Length;

        // Try to break at sentence end
        int lastPeriod = text.LastIndexOf('.', maxLength);
        int lastExclamation = text.LastIndexOf('!', maxLength);
        int lastQuestion = text.LastIndexOf('?', maxLength);

        int sentenceBreak = Mathf.Max(lastPeriod, lastExclamation, lastQuestion);
        if (sentenceBreak > maxLength / 2) // Only use if it's not too early
            return sentenceBreak + 1;

        // Try to break at space
        int lastSpace = text.LastIndexOf(' ', maxLength);
        if (lastSpace > 0)
            return lastSpace;

        // Force break at max length
        return maxLength;
    }

    private void DisplayCurrentSegment()
    {
        if (currentConversation == null)
            return;

        DialogueLine currentLine = currentConversation.dialogueLines[currentLineIndex];
        string textToDisplay = currentLine.textSegments[currentSegmentIndex];

        // Update speaker sprite
        if (speakerImage != null && currentLine.speakerSprite != null)
        {
            speakerImage.sprite = currentLine.speakerSprite;
            speakerImage.enabled = true;
        }
        else if (speakerImage != null)
        {
            speakerImage.enabled = false;
        }

        // Play voice clip (only on first segment of a line)
        if (currentSegmentIndex == 0 && autoPlayVoice &&
            currentLine.voiceClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentLine.voiceClip);
        }

        // Display text
        if (useTypewriter && dialogueSystem != null)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterEffect(textToDisplay));
        }
        else if (dialogueSystem != null)
        {
            dialogueSystem.DisplayDialogue(textToDisplay);
        }

        // Update navigation buttons
        UpdateNavigationButtons();

        // Invoke event
        int absoluteSegmentIndex = GetAbsoluteSegmentIndex();
        OnSegmentChange?.Invoke(absoluteSegmentIndex + 1, totalSegments);
    }

    private System.Collections.IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        string displayedText = "";

        // Parse the text to handle brackets properly
        for (int i = 0; i < text.Length; i++)
        {
            displayedText += text[i];

            if (dialogueSystem != null)
                dialogueSystem.DisplayDialogue(displayedText);

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    public void NextSegment()
    {
        if (currentConversation == null || isTyping)
            return;

        DialogueLine currentLine = currentConversation.dialogueLines[currentLineIndex];

        // Move to next segment in current line
        if (currentSegmentIndex < currentLine.textSegments.Count - 1)
        {
            currentSegmentIndex++;
            DisplayCurrentSegment();
        }
        // Move to next line
        else if (currentLineIndex < currentConversation.dialogueLines.Count - 1)
        {
            currentLineIndex++;
            currentSegmentIndex = 0;
            DisplayCurrentSegment();
        }
        // End of conversation
        else
        {
            EndConversation();
        }
    }

    public void PreviousSegment()
    {
        if (currentConversation == null || isTyping)
            return;

        // Move to previous segment in current line
        if (currentSegmentIndex > 0)
        {
            currentSegmentIndex--;
            DisplayCurrentSegment();
        }
        // Move to previous line (last segment)
        else if (currentLineIndex > 0)
        {
            currentLineIndex--;
            DialogueLine previousLine = currentConversation.dialogueLines[currentLineIndex];
            currentSegmentIndex = previousLine.textSegments.Count - 1;
            DisplayCurrentSegment();
        }
    }

    private bool CanGoNext()
    {
        if (currentConversation == null)
            return false;

        DialogueLine currentLine = currentConversation.dialogueLines[currentLineIndex];
        bool isLastSegment = currentSegmentIndex >= currentLine.textSegments.Count - 1;
        bool isLastLine = currentLineIndex >= currentConversation.dialogueLines.Count - 1;

        return !(isLastSegment && isLastLine);
    }

    private bool CanGoPrevious()
    {
        return currentLineIndex > 0 || currentSegmentIndex > 0;
    }

    private void UpdateNavigationButtons()
    {

        // Navigation state can be checked with CanGoNext() and CanGoPrevious()
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
        OnConversationEnd?.Invoke();

        if (conversationPanel != null)
            conversationPanel.SetActive(false);

        currentConversation = null;
        currentLineIndex = 0;
        currentSegmentIndex = 0;
    }

    public void SkipTypewriter()
    {
        if (isTyping && typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            isTyping = false;

            DialogueLine currentLine = currentConversation.dialogueLines[currentLineIndex];
            string fullText = currentLine.textSegments[currentSegmentIndex];

            if (dialogueSystem != null)
                dialogueSystem.DisplayDialogue(fullText);
        }
    }

    // Helper method to manually play voice
    public void PlayCurrentVoice()
    {
        if (currentConversation == null)
            return;

        DialogueLine currentLine = currentConversation.dialogueLines[currentLineIndex];
        if (currentLine.voiceClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentLine.voiceClip);
        }
    }
    private void DisableCamera()
    {
        ThirdPersonCameraController camera = Camera.main.gameObject.GetComponent<ThirdPersonCameraController>();
        if (camera != null)
        {
            camera.SetCameraRotationEnabled(false);
            camera.target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        }
    }

    private void EnableCamera()
    {
        ThirdPersonCameraController camera = Camera.main.gameObject.GetComponent<ThirdPersonCameraController>();
        if (camera != null)
        {
            camera.SetCameraRotationEnabled(true);
            camera.target.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }
}
