using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Encyclopedia encyclopedia;
    [SerializeField] private EncyclopediaPanel encyclopediaPanel;

    [Header("Visual Settings")]
    [SerializeField] private Color loreColor = new Color(0.8f, 0.6f, 1f);
    [SerializeField] private Color gameSystemColor = new Color(0.3f, 0.8f, 1f);

    [Header("Navigation Settings")]
    [SerializeField] private bool enableTermNavigation = true;
    [SerializeField] private GameObject termSelectionIndicator;

    // State
    private List<LinkedTerm> linkedTerms = new List<LinkedTerm>();
    private int currentTermIndex = -1;
    private bool hasTermsInCurrentDialogue;

    // Cache
    private static readonly Regex BracketRegex = new Regex(@"\[([^\]]+)\]", RegexOptions.Compiled);

    // Constants
    private const int INVALID_TERM_INDEX = -1;
    private const int FIRST_TERM_INDEX = 0;
    private const string LINK_ID_PREFIX = "link_";

    private struct LinkedTerm
    {
        public string term;
        public string linkId;
        public EncyclopediaEntry entry;
        public int linkIndex;
    }

    void Start()
    {
        InitializeEncyclopedia();
    }

    private void InitializeEncyclopedia()
    {
        if (encyclopedia != null)
        {
            encyclopedia.Initialize();
        }
    }

    public void DisplayDialogue(string dialogue)
    {
        ProcessDialogue(dialogue);
    }

    private void ProcessDialogue(string dialogue)
    {
        ResetDialogueState();

        MatchCollection matches = BracketRegex.Matches(dialogue);

        if (matches.Count == 0)
        {
            DisplayPlainText(dialogue);
            return;
        }

        string processedText = ProcessMatches(dialogue, matches);
        DisplayProcessedText(processedText);

        if (hasTermsInCurrentDialogue && enableTermNavigation)
        {
            SelectFirstTerm();
        }
    }

    private void ResetDialogueState()
    {
        linkedTerms.Clear();
        currentTermIndex = INVALID_TERM_INDEX;
        hasTermsInCurrentDialogue = false;
    }

    private void DisplayPlainText(string text)
    {
        dialogueText.text = text;
    }

    private string ProcessMatches(string dialogue, MatchCollection matches)
    {
        string processedText = dialogue;
        int linkCounter = 0;

        // Process in reverse to maintain string positions
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            Match match = matches[i];
            string term = match.Groups[1].Value;
            EncyclopediaEntry entry = GetEncyclopediaEntry(term);

            if (entry != null)
            {
                processedText = ProcessMatchedTerm(processedText, match, term, entry, linkCounter);
                linkCounter++;
            }
            else
            {
                processedText = RemoveBrackets(processedText, match, term);
            }
        }

        return processedText;
    }

    private EncyclopediaEntry GetEncyclopediaEntry(string term)
    {
        return encyclopedia?.GetEntry(term);
    }

    private string ProcessMatchedTerm(string text, Match match, string term, EncyclopediaEntry entry, int linkCounter)
    {
        string linkId = LINK_ID_PREFIX + linkCounter;
        Color termColor = GetTermColor(entry.type);
        string linkedText = CreateLinkedText(term, linkId, termColor);

        text = ReplaceMatchWithLinkedText(text, match, linkedText);
        StoreLinkedTerm(term, linkId, entry, linkCounter);

        hasTermsInCurrentDialogue = true;

        return text;
    }

    private Color GetTermColor(EntryType type)
    {
        return type == EntryType.Lore ? loreColor : gameSystemColor;
    }

    private string CreateLinkedText(string term, string linkId, Color color)
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(color);
        return $"<link=\"{linkId}\"><color=#{colorHex}><u>{term}</u></color></link>";
    }

    private string ReplaceMatchWithLinkedText(string text, Match match, string linkedText)
    {
        text = text.Remove(match.Index, match.Length);
        text = text.Insert(match.Index, linkedText);
        return text;
    }

    private void StoreLinkedTerm(string term, string linkId, EncyclopediaEntry entry, int linkCounter)
    {
        linkedTerms.Insert(0, new LinkedTerm
        {
            term = term,
            linkId = linkId,
            entry = entry,
            linkIndex = linkCounter
        });
    }

    private string RemoveBrackets(string text, Match match, string term)
    {
        text = text.Remove(match.Index, match.Length);
        text = text.Insert(match.Index, term);
        return text;
    }

    private void DisplayProcessedText(string text)
    {
        dialogueText.text = text;
    }

    private void SelectFirstTerm()
    {
        currentTermIndex = FIRST_TERM_INDEX;
        UpdateTermIndicator();
    }

    void Update()
    {
        if (!ShouldProcessInput())
        {
            return;
        }

        HandleMouseClick();
    }

    private bool ShouldProcessInput()
    {
        return enableTermNavigation && hasTermsInCurrentDialogue;
    }

    private void HandleMouseClick()
    {
        if (!IsMouseButtonPressed())
        {
            return;
        }

        int linkIndex = GetClickedLinkIndex();

        if (linkIndex != INVALID_TERM_INDEX)
        {
            ProcessClickedLink(linkIndex);
        }
    }

    private bool IsMouseButtonPressed()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private int GetClickedLinkIndex()
    {
        if (dialogueText == null)
        {
            return INVALID_TERM_INDEX;
        }

        return TMP_TextUtilities.FindIntersectingLink(
            dialogueText,
            Mouse.current.position.ReadValue(),
            null
        );
    }

    private void ProcessClickedLink(int linkIndex)
    {
        TMP_LinkInfo linkInfo = dialogueText.textInfo.linkInfo[linkIndex];
        string linkId = linkInfo.GetLinkID();

        int termIndex = FindTermIndexByLinkId(linkId);

        if (termIndex != INVALID_TERM_INDEX)
        {
            currentTermIndex = termIndex;
            OpenCurrentTerm();
        }
    }

    private int FindTermIndexByLinkId(string linkId)
    {
        for (int i = 0; i < linkedTerms.Count; i++)
        {
            if (linkedTerms[i].linkId == linkId)
            {
                return i;
            }
        }

        return INVALID_TERM_INDEX;
    }

    public void CycleTermForward()
    {
        if (!CanNavigateTerms())
        {
            return;
        }

        currentTermIndex = (currentTermIndex + 1) % linkedTerms.Count;
        UpdateTermIndicator();
    }

    public void CycleTermBackward()
    {
        if (!CanNavigateTerms())
        {
            return;
        }

        currentTermIndex--;

        if (currentTermIndex < 0)
        {
            currentTermIndex = linkedTerms.Count - 1;
        }

        UpdateTermIndicator();
    }

    private bool CanNavigateTerms()
    {
        return hasTermsInCurrentDialogue && linkedTerms.Count > 0;
    }

    public void OpenCurrentTerm()
    {
        if (!IsValidTermSelected())
        {
            return;
        }

        LinkedTerm term = linkedTerms[currentTermIndex];
        OpenEncyclopediaEntry(term.entry);
    }

    private bool IsValidTermSelected()
    {
        return hasTermsInCurrentDialogue &&
               currentTermIndex >= 0 &&
               currentTermIndex < linkedTerms.Count;
    }

    private void UpdateTermIndicator()
    {
        if (!ShouldShowIndicator())
        {
            HideIndicator();
            return;
        }

        if (!IsValidTermSelected())
        {
            HideIndicator();
            return;
        }

        PositionIndicator();
    }

    private bool ShouldShowIndicator()
    {
        return termSelectionIndicator != null && dialogueText != null;
    }

    private void HideIndicator()
    {
        if (termSelectionIndicator != null)
        {
            termSelectionIndicator.SetActive(false);
        }
    }

    private void PositionIndicator()
    {
        dialogueText.ForceMeshUpdate();

        LinkedTerm currentTerm = linkedTerms[currentTermIndex];
        TMP_LinkInfo linkInfo = dialogueText.textInfo.linkInfo[currentTerm.linkIndex];

        int firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
        int lastCharIndex = linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength - 1;

        if (firstCharIndex < dialogueText.textInfo.characterCount)
        {
            Vector3 center = CalculateTermCenter(firstCharIndex, lastCharIndex);
            ShowIndicatorAt(center);
        }
    }

    private Vector3 CalculateTermCenter(int firstCharIndex, int lastCharIndex)
    {
        Vector3 bottomLeft = dialogueText.textInfo.characterInfo[firstCharIndex].bottomLeft;
        Vector3 topRight = dialogueText.textInfo.characterInfo[lastCharIndex].topRight;

        return (bottomLeft + topRight) / 2f;
    }

    private void ShowIndicatorAt(Vector3 localPosition)
    {
        termSelectionIndicator.SetActive(true);
        termSelectionIndicator.transform.position = dialogueText.transform.TransformPoint(localPosition);
    }

    private void OpenEncyclopediaEntry(EncyclopediaEntry entry)
    {
        if (encyclopediaPanel != null)
        {
            encyclopediaPanel.DisplayEntry(entry);
        }
    }

    public bool HasSelectableTerms()
    {
        return hasTermsInCurrentDialogue;
    }

    public int GetCurrentTermIndex()
    {
        return currentTermIndex;
    }

    public int GetTermCount()
    {
        return linkedTerms.Count;
    }
}
