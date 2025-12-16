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

    [Header("Visual Settings")]
    [SerializeField] private Color loreColor = new Color(0.8f, 0.6f, 1f); // Purple
    [SerializeField] private Color gameSystemColor = new Color(0.3f, 0.8f, 1f); // Cyan

    [Header("Events")]
    public UnityEvent<EncyclopediaEntry> OnEntryClicked;

    private List<LinkedTerm> linkedTerms = new List<LinkedTerm>();

    private struct LinkedTerm
    {
        public string term;
        public string linkId;
        public EncyclopediaEntry entry;
    }

    void Start()
    {
        if (encyclopedia != null)
            encyclopedia.Initialize();

        // Enable link clicking
        if (dialogueText != null)
        {
            dialogueText.raycastTarget = true;
        }
    }

    public void DisplayDialogue(string dialogue)
    {
        ProcessDialogue(dialogue);
    }

    private void ProcessDialogue(string dialogue)
    {
        linkedTerms.Clear();

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
                string linkId = $"link_{linkCounter++}";
                Color termColor = entry.type == EntryType.Lore ? loreColor : gameSystemColor;

                // Create clickable link with TMP link tags
                string linkedText = $"<link=\"{linkId}\"><color=#{ColorUtility.ToHtmlStringRGB(termColor)}><u>{term}</u></color></link>";

                // Replace [term] with linked version
                processedText = processedText.Remove(match.Index, match.Length);
                processedText = processedText.Insert(match.Index, linkedText);

                // Store term info
                linkedTerms.Add(new LinkedTerm
                {
                    term = term,
                    linkId = linkId,
                    entry = entry
                });
            }
            else
            {
                // Remove brackets if no entry found
                processedText = processedText.Remove(match.Index, match.Length);
                processedText = processedText.Insert(match.Index, term);
            }
        }

        dialogueText.text = processedText;
    }

    void LateUpdate()
    {
        // Detect link clicks (works with both old and new input systems)
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

                // Find the entry for this link
                foreach (var linkedTerm in linkedTerms)
                {
                    if (linkedTerm.linkId == linkId)
                    {
                        OpenEncyclopediaEntry(linkedTerm.entry);
                        break;
                    }
                }
            }
        }
    }

    private void OpenEncyclopediaEntry(EncyclopediaEntry entry)
    {
        Debug.Log($"Opening encyclopedia entry: {entry.title} (Type: {entry.type})");

        // Invoke event for UI system to handle
        OnEntryClicked?.Invoke(entry);
    }

    // Example: Call this from your input system or UI button
    public void ShowExampleDialogue()
    {
        DisplayDialogue("The [Dragon King] used [Fire Magic] to destroy the village. Learn about [Combat] to defeat him.");
    }
}
