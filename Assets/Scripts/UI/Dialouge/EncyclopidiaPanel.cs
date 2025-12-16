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
    [SerializeField] private Color loreTypeColor = new Color(0.8f, 0.6f, 1f); // Purple
    [SerializeField] private Color gameSystemTypeColor = new Color(0.3f, 0.8f, 1f); // Cyan
    [SerializeField] private string loreLabel = "LORE";
    [SerializeField] private string gameSystemLabel = "GAME SYSTEM";

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void DisplayEntry(EncyclopediaEntry entry)
    {
        if (panel != null)
            panel.SetActive(true);

        if (titleText != null)
            titleText.text = entry.title;

        if (typeText != null)
        {
            typeText.text = entry.type == EntryType.Lore ? loreLabel : gameSystemLabel;
            typeText.color = entry.type == EntryType.Lore ? loreTypeColor : gameSystemTypeColor;
        }

        if (descriptionText != null)
            descriptionText.text = entry.description;

        // Show mechanics info only for game systems
        if (mechanicsSection != null)
            mechanicsSection.SetActive(entry.type == EntryType.GameSystem);

        if (mechanicsText != null && entry.type == EntryType.GameSystem)
            mechanicsText.text = entry.mechanicsInfo;

        if (iconImage != null && entry.icon != null)
        {
            iconImage.sprite = entry.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
