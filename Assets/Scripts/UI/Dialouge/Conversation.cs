using UnityEngine;
using System.Collections.Generic;


// Single dialogue line in a conversation
[System.Serializable]
public class DialogueLine
{
    public Sprite speakerSprite;
    public AudioClip voiceClip;
    [TextArea(3, 10)]
    public string dialogueText;

    [HideInInspector]
    public List<string> textSegments = new List<string>();
}

// A complete conversation containing multiple dialogue lines
[CreateAssetMenu(fileName = "NewConversation", menuName = "Dialogue/Conversation")]
public class Conversation : ScriptableObject
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}
