using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays item information and stat changes when hovering over equipment
/// </summary>
public class EquipmentInfoDisplay : MonoBehaviour
{
    [Header("Stat Panels")]
    [SerializeField] private GameObject chuzpahPanel;
    [SerializeField] private GameObject cadishnessPanel;
    [SerializeField] private GameObject gracePanel;
    [SerializeField] private GameObject gritPanel;
    [SerializeField] private GameObject serindipityPanel;
    [SerializeField] private GameObject swaggerPanel;

    [Header("Info Text")]
    [SerializeField] private TextMeshProUGUI infoText;

    private Image[] statPanelImages;

    void Start()
    {
        InitializeStatPanels();

        EventManager.OnInfoChange += DisplayItemInfo;
        EventManager.OnInfoReset += ResetDisplay;
        EventManager.OnInfoCompareChange += DisplayItemComparison;
    }

    private void InitializeStatPanels()
    {
        statPanelImages = new Image[]
        {
            chuzpahPanel.GetComponent<Image>(),
            cadishnessPanel.GetComponent<Image>(),
            gracePanel.GetComponent<Image>(),
            gritPanel.GetComponent<Image>(),
            serindipityPanel.GetComponent<Image>(),
            swaggerPanel.GetComponent<Image>()
        };
    }

    private void DisplayItemInfo(Item item)
    {
        if (item == null) return;

        UpdateStatPanels(item.StatChanges);
        UpdateInfoText(item, item.StatChanges);
    }

    private void DisplayItemComparison(Item item, int[] statChanges)
    {
        if (item == null) return;

        UpdateStatPanels(statChanges);
        UpdateInfoText(item, statChanges);
    }

    private void UpdateStatPanels(int[] statChanges)
    {
        if (statPanelImages == null || statChanges == null) return;

        for (int i = 0; i < statPanelImages.Length && i < statChanges.Length; i++)
        {
            statPanelImages[i].color = GetStatColor(statChanges[i]);
        }
    }

    private Color GetStatColor(int value)
    {
        if (value > 0) return Color.green;
        if (value < 0) return Color.red;
        return Color.gray;
    }

    private void UpdateInfoText(Item item, int[] statChanges)
    {
        infoText.text = $"{item.Info}{Environment.NewLine}{string.Join(" ", statChanges)}";
    }

    public void ResetDisplay()
    {
        if (statPanelImages == null) return;

        foreach (Image panel in statPanelImages)
        {
            panel.color = Color.gray;
        }

        if (infoText != null)
            infoText.text = "";
    }

    void OnDestroy()
    {
        EventManager.OnInfoChange -= DisplayItemInfo;
        EventManager.OnInfoReset -= ResetDisplay;
        EventManager.OnInfoCompareChange -= DisplayItemComparison;
    }
}
