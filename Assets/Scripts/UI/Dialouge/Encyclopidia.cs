using UnityEngine;
using System.Collections.Generic;


// Entry types for different systems
public enum EntryType
{
    Lore,
    GameSystem
}

// Data structure for encyclopedia/game system entries
[System.Serializable]
public class EncyclopediaEntry
{
    public string key;
    public EntryType type;
    [TextArea(3, 6)]
    public string title;
    [TextArea(5, 10)]
    public string description;
    public Sprite icon;
    [TextArea(2, 4)]
    public string mechanicsInfo; // For game systems
    public List<string> relatedEntries; // Cross-references
}

// ScriptableObject to store all entries
[CreateAssetMenu(fileName = "Encyclopedia", menuName = "Dialogue/Encyclopedia")]
public class Encyclopedia : ScriptableObject
{
    public List<EncyclopediaEntry> entries = new List<EncyclopediaEntry>();

    private Dictionary<string, EncyclopediaEntry> _lookup;

    public void Initialize()
    {
        _lookup = new Dictionary<string, EncyclopediaEntry>();
        foreach (var entry in entries)
        {
            _lookup[entry.key.ToLower()] = entry;
        }
    }

    public EncyclopediaEntry GetEntry(string key)
    {
        if (_lookup == null) Initialize();
        return _lookup.TryGetValue(key.ToLower(), out var entry) ? entry : null;
    }

    public List<EncyclopediaEntry> GetEntriesByType(EntryType type)
    {
        return entries.FindAll(e => e.type == type);
    }
}

