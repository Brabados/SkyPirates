using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TileTypeGenerator : MonoBehaviour
{
    public RectTransform ScrollSpace;
    public GameObject Viewport;
    public List<TileDataSO> TileData;
    public Button Prefab;
    private List<Button> TileList = new List<Button>(); // Initialize the list at the time of declaration

    // Start is called before the first frame update
    void Start()
    {
        TileData = new List<TileDataSO>();
        Map Playarea = FindObjectOfType<Map>();
        foreach(TileDataSO a in Playarea.TileTypes)
        {
            TileData.Add(a);
        }
        SpawnButtons();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnButtons()
    {
        Viewport.GetComponent<RectTransform>().sizeDelta = new Vector2(
    Viewport.GetComponent<RectTransform>().sizeDelta.x,
    TileData.Count * (ScrollSpace.rect.height / 6) + 150);
        // Create and position the buttons
        for (int x = 0; x < TileData.Count; x++)
        {
            Button generatedButton = CreateButton(TileData[x]);
            generatedButton.GetComponentInChildren<Text>().text = TileData[x].Name;

            // Position buttons at the top of the scroll area
            generatedButton.transform.localPosition = new Vector3(
                0 + (ScrollSpace.rect.width / 2),
                0 - (x * (ScrollSpace.rect.height / 6) + ScrollSpace.rect.height / 6),
                0);
            TileList.Add(generatedButton);
        }

        EventSystem.current.SetSelectedGameObject(TileList[0].gameObject);
    }

    public Button CreateButton(TileDataSO item)
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
        TileTypeButton newScript = button.gameObject.AddComponent<TileTypeButton>();
        newScript.tile = item;
        button.onClick.AddListener(newScript.SetChange);

        return button;
    }
}
