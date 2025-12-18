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
    [SerializeField] private Color loreColor = new Color(0.8f, 0.6f, 1f); // Purple
    [SerializeField] private Color gameSystemColor = new Color(0.3f, 0.8f, 1f); // Cyan
    [SerializeField] private Color selectedTermColor = Color.white;

    [Header("Navigation Settings")]
    [SerializeField] private bool enableTermNavigation = true;
    [Tooltip("Visual indicator for currently selected term (optional)")]
    [SerializeField] private GameObject termSelectionIndicator;

    private List<LinkedTerm> linkedTerms = new List<LinkedTerm>();
    private int currentTermIndex = -1;
    private bool hasTermsInCurrentDialogue = false;

    private struct LinkedTerm
    {
        public string term;
        public string linkId;
        public EncyclopediaEntry entry;
        public int linkIndex; // TMP link index for highlighting
    }

    void Start()
    {
        if (encyclopedia != null)
            encyclopedia.Initialize();
    }

    public void DisplayDialogue(string dialogue)
    {
        ProcessDialogue(dialogue);
    }

    private void ProcessDialogue(string dialogue)
    {
        linkedTerms.Clear();
        currentTermIndex = -1;
        hasTermsInCurrentDialogue = false;

        // Find all bracketed terms
        MatchCollection matches = Regex.Matches(dialogue, @"\[([^\]]+)\]");

        string processedText = dialogue;
        int linkCounter = 0;

        // Process matches in reverse to maintain string positions
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            Match match = matches[i];
            string term = match.Groups[1].Value;
            EncyclopediaEntry entry = encyclopedia?.GetEntry(term);

            if (entry != null)
            {
                string linkId = $"link_{linkCounter}";
                Color termColor = entry.type == EntryType.Lore ? loreColor : gameSystemColor;

                // Create clickable link with TMP link tags
                string linkedText = $"<link=\"{linkId}\"><color=#{ColorUtility.ToHtmlStringRGB(termColor)}><u>{term}</u></color></link>";

                // Replace [term] with linked version
                processedText = processedText.Remove(match.Index, match.Length);
                processedText = processedText.Insert(match.Index, linkedText);

                // Store term info (add to front since we're processing in reverse)
                linkedTerms.Insert(0, new LinkedTerm
                {
                    term = term,
                    linkId = linkId,
                    entry = entry,
                    linkIndex = linkCounter
                });

                linkCounter++;
                hasTermsInCurrentDialogue = true;
            }
            else
            {
                // Remove brackets if no entry found
                processedText = processedText.Remove(match.Index, match.Length);
                processedText = processedText.Insert(match.Index, term);
            }
        }

        dialogueText.text = processedText;

        // Auto-select first term if available
        if (hasTermsInCurrentDialogue && enableTermNavigation)
        {
            currentTermIndex = 0;
            HighlightCurrentTerm();
        }
    }

    void Update()
    {
        if (!enableTermNavigation || !hasTermsInCurrentDialogue)
            return;

        // Mouse/touch click support
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && dialogueText != null)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                dialogueText,
                Mouse.current.position.ReadValue(),
                null
            );

            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = dialogueText.textInfo.linkInfo[linkIndex];
                string linkId = linkInfo.GetLinkID();

                // Find and open the entry for this link
                for (int i = 0; i < linkedTerms.Count; i++)
                {
                    if (linkedTerms[i].linkId == linkId)
                    {
                        currentTermIndex = i;
                        OpenCurrentTerm();
                        break;
                    }
                }
            }
        }
    }

    public void CycleTermForward()
    {
        if (!hasTermsInCurrentDialogue || linkedTerms.Count == 0)
            return;

        currentTermIndex = (currentTermIndex + 1) % linkedTerms.Count;
        HighlightCurrentTerm();
    }

    public void CycleTermBackward()
    {
        if (!hasTermsInCurrentDialogue || linkedTerms.Count == 0)
            return;

        currentTermIndex--;
        if (currentTermIndex < 0)
            currentTermIndex = linkedTerms.Count - 1;

        HighlightCurrentTerm();
    }

    public void OpenCurrentTerm()
    {
        if (!hasTermsInCurrentDialogue || currentTermIndex < 0 || currentTermIndex >= linkedTerms.Count)
            return;

        LinkedTerm term = linkedTerms[currentTermIndex];
        OpenEncyclopediaEntry(term.entry);
    }

    private void HighlightCurrentTerm()
    {
        if (currentTermIndex < 0 || currentTermIndex >= linkedTerms.Count)
            return;

        // Rebuild text with current term highlighted differently
        string currentText = dialogueText.text;

        UpdateTermIndicator();
    }

    private void UpdateTermIndicator()
    {
        if (termSelectionIndicator == null || dialogueText == null)
            return;

        if (currentTermIndex < 0 || currentTermIndex >= linkedTerms.Count)
        {
            termSelectionIndicator.SetActive(false);
            return;
        }

        // Force text mesh to update
        dialogueText.ForceMeshUpdate();

        LinkedTerm currentTerm = linkedTerms[currentTermIndex];
        TMP_LinkInfo linkInfo = dialogueText.textInfo.linkInfo[currentTerm.linkIndex];

        // Get the bounds of the linked text
        int firstCharIndex = linkInfo.linkTextfirstCharacterIndex;
        int lastCharIndex = linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength - 1;

        if (firstCharIndex < dialogueText.textInfo.characterCount)
        {
            Vector3 bottomLeft = dialogueText.textInfo.characterInfo[firstCharIndex].bottomLeft;
            Vector3 topRight = dialogueText.textInfo.characterInfo[lastCharIndex].topRight;

            Vector3 center = (bottomLeft + topRight) / 2;

            termSelectionIndicator.SetActive(true);
            termSelectionIndicator.transform.position = dialogueText.transform.TransformPoint(center);
        }
    }

    private void OpenEncyclopediaEntry(EncyclopediaEntry entry)
    {
        if (encyclopediaPanel != null)
        {
            encyclopediaPanel.DisplayEntry(entry);
        }
    }

    public void ShowExampleDialogue()
    {
        DisplayDialogue("The [Dragon King] used [Fire Magic] to destroy the village. Learn about [Combat] to defeat him.");
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
