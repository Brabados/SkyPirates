using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentChangeUpdater : MonoBehaviour
{
    public GameObject ChuzpahPanel;
    public GameObject CadishnessPanel;
    public GameObject GracePanel;
    public GameObject GritPanel;
    public GameObject SerindipityPanel;
    public GameObject SwaggerPanel;

    private Image[] statImages;

    public Text InfoText;

    public void Start()
    {
        EventManager.OnInfoChange += InfoChange;
        EventManager.OnInfoReset += ResetPanels;
        EventManager.OnInfoCompareChange += InfoChangeCompare;

        GetImage();
    }

    public void GetImage()
    {
        statImages = new Image[6];
        statImages[0] = ChuzpahPanel.GetComponent<Image>();
        statImages[1] = CadishnessPanel.GetComponent<Image>();
        statImages[2] = GracePanel.GetComponent<Image>();
        statImages[3] = GritPanel.GetComponent<Image>();
        statImages[4] = SerindipityPanel.GetComponent<Image>();
        statImages[5] = SwaggerPanel.GetComponent<Image>();
    }

    public void PanelChange(int[] Colors)
    {
        EnsureStatImagesInitialized();

        for (int i = 0; i < statImages.Length && i < Colors.Length; i++)
        {
            if (Colors[i] > 0)
                statImages[i].color = Color.green;
            else if (Colors[i] < 0)
                statImages[i].color = Color.red;
            else
                statImages[i].color = Color.gray; // Explicitly reset to neutral
        }
    }

    public void InfoChange(Item Info)
    {
        PanelChange(Info.StatChanges);
        InfoText.text = $"{Info.Info}{System.Environment.NewLine}{string.Join(" ", Info.StatChanges)}";
    }

    public void InfoChangeCompare(Item Info, int[] StatChanges)
    {
        PanelChange(StatChanges);
        InfoText.text = $"{Info.Info}{System.Environment.NewLine}{string.Join(" ", Info.StatChanges)}";
    }

    public void ResetPanels()
    {
        EnsureStatImagesInitialized();

        foreach (var img in statImages)
        {
            img.color = Color.gray;
        }
    }

    public void OnDestroy()
    {
        EventManager.OnInfoChange -= InfoChange;
        EventManager.OnInfoReset -= ResetPanels;
        EventManager.OnInfoCompareChange -= InfoChangeCompare;
    }

    private void EnsureStatImagesInitialized()
    {
        if (statImages == null || statImages.Length == 0)
        {
            GetImage();
        }
    }
}
