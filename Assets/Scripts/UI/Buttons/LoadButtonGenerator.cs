using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadButtonGenerator : MonoBehaviour
{
    public RectTransform ScrollSpace;
    public GameObject Viewport;
    public Inventory PublicItems;
    public Button Prefab;
    private List<Button> Loadlist = new List<Button>();

    public void Start()
    {
        SpawnButtons();
    }

    public void SpawnButtons()
    {
        int count = 0;
        // Create and position the buttons
        foreach (FileInfo f in SaveLoadManager.SaveLoadInstance.info)
        {
            Button generatedButton = CreateButton(f);
            generatedButton.GetComponentInChildren<Text>().text = f.Name;

            // Position buttons at the top of the scroll area
            generatedButton.transform.position = new Vector3(
                generatedButton.transform.position.x,
                ScrollSpace.rect.y - (count * (ScrollSpace.rect.height / 6)),
                0);

            Loadlist.Add(generatedButton);
            count++;
        }
    }

    public Button CreateButton(FileInfo file)
    {
        Button button = Instantiate(Prefab, Vector3.zero, Quaternion.identity);
        RectTransform rectTransform = button.GetComponent<RectTransform>();

        // Setup RectTransform properties
        rectTransform.SetParent(Viewport.transform);
        rectTransform.anchorMax = this.GetComponent<RectTransform>().anchorMax;
        rectTransform.anchorMin = this.GetComponent<RectTransform>().anchorMin;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(ScrollSpace.rect.width, ScrollSpace.rect.height / 6);

        // Add custom script and listener
        TempLoad newScript = button.gameObject.AddComponent<TempLoad>();
        newScript.Load = file.Name;
        newScript.ToLoad = FindObjectOfType<Map>();
        button.onClick.AddListener(newScript.Press);

        return button;
    }
}
