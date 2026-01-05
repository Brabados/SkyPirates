using UnityEngine;
using TMPro;
public class EncyclopediaPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI mechanicsText;
    [SerializeField] private GameObject mechanicsSection;
    [SerializeField] private UnityEngine.UI.Image iconImage;

    [Header("Visual Settings")]
    [SerializeField] private Color loreTypeColor = new Color(0.8f, 0.6f, 1f);
    [SerializeField] private Color gameSystemTypeColor = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private string loreLabel = "LORE";
    [SerializeField] private string gameSystemLabel = "GAME SYSTEM";

    private bool isPanelOpen;

    void Start()
    {
        InitializePanel();
    }

    private void InitializePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void DisplayEntry(EncyclopediaEntry entry)
    {
        ShowPanel();
        PopulateEntryData(entry);
    }

    private void ShowPanel()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }

        isPanelOpen = true;
    }

    private void PopulateEntryData(EncyclopediaEntry entry)
    {
        SetTitle(entry.title);
        SetTypeLabel(entry.type);
        SetDescription(entry.description);
        SetMechanicsInfo(entry);
        SetIcon(entry.icon);
    }

    private void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    private void SetTypeLabel(EntryType type)
    {
        if (typeText == null)
        {
            return;
        }

        typeText.text = GetTypeLabel(type);
        typeText.color = GetTypeColor(type);
    }

    private string GetTypeLabel(EntryType type)
    {
        return type == EntryType.Lore ? loreLabel : gameSystemLabel;
    }

    private Color GetTypeColor(EntryType type)
    {
        return type == EntryType.Lore ? loreTypeColor : gameSystemTypeColor;
    }

    private void SetDescription(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
    }

    private void SetMechanicsInfo(EncyclopediaEntry entry)
    {
        bool isGameSystem = entry.type == EntryType.GameSystem;

        if (mechanicsSection != null)
        {
            mechanicsSection.SetActive(isGameSystem);
        }

        if (mechanicsText != null && isGameSystem)
        {
            mechanicsText.text = entry.mechanicsInfo;
        }
    }

    private void SetIcon(Sprite icon)
    {
        if (iconImage == null)
        {
            return;
        }

        if (icon != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(true);
        }
        else
        {
            iconImage.gameObject.SetActive(false);
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        isPanelOpen = false;
    }

    public bool IsPanelOpen()
    {
        return isPanelOpen;
    }
}
