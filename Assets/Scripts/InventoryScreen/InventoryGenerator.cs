using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class InventoryGenerator : MonoBehaviour
{
    public RectTransform ScrollSpace;
    public GameObject Viewport;
    public Inventory PublicItems;
    public Button Prefab;
    private List<Button> Inventorylist = new List<Button>(); // Initialize the list at the time of declaration

    // Start is called before the first frame update
    void Start()
    {
        SpawnButtons();
    }

    public void SpawnButtons()
    {
        Viewport.GetComponent<RectTransform>().sizeDelta = new Vector2(
        Viewport.GetComponent<RectTransform>().sizeDelta.x,
        PublicItems.InInventory.Count * (ScrollSpace.rect.height / 6) + 150);
        // Create and position the buttons
        for (int x = 0; x < PublicItems.InInventory.Count; x++)
        {
            Button generatedButton = CreateButton(PublicItems.InInventory[x]);
            generatedButton.GetComponentInChildren<Text>().text = PublicItems.InInventory[x].Name;

            // Position buttons at the top of the scroll area
            generatedButton.transform.localPosition = new Vector3(
                0 - (ScrollSpace.rect.width/2),
                0 - (x * (ScrollSpace.rect.height / 6) + ScrollSpace.rect.height / 6),
                0);
            Inventorylist.Add(generatedButton);
        }

        EventSystem.current.SetSelectedGameObject(Inventorylist[0].gameObject);
    }

    public Button CreateButton(Item item)
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
        InventoryItemButton newScript = button.gameObject.AddComponent<InventoryItemButton>();
        newScript.Equip = item;
        button.onClick.AddListener(newScript.onClick);

        return button;
    }
}
