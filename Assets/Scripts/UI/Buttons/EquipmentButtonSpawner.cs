using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class EquipmentButtonSpawner : MonoBehaviour
{
    public RectTransform ScrollSpace;
    public GameObject Viewport;
    public Inventory PublicItems;
    public Button Prefab;
    private List<Button> Inventorylist = new List<Button>(); // Initialize the list at the time of declaration

    void Start()
    {
        EventManager.OnItemSelect += SpawnButtons;
        EventManager.OnEquipmentChange += Equipmentchangebuttonpress;
    }

    public void SpawnButtons(Item Search)
    {
        // Clear and reset the inventory list
        ClearInventoryList();

        // Filter items based on the search criteria
        List<Item> SearchResults = GetFilteredItems(Search);

        // Create and position the buttons
        for (int x = 0; x < SearchResults.Count; x++)
        {
            Button generatedButton = CreateButton(SearchResults[x]);
            generatedButton.GetComponentInChildren<Text>().text = SearchResults[x].Name;

            // Position buttons at the top of the scroll area
            generatedButton.transform.position = new Vector3(
                generatedButton.transform.position.x,
                ScrollSpace.rect.height - (x * (ScrollSpace.rect.height / 6)),
                0);

            Inventorylist.Add(generatedButton);
        }
        EventSystem.current.SetSelectedGameObject(Inventorylist[0].gameObject);
    }

    // Function to clear the list of existing buttons
    private void ClearInventoryList()
    {
        foreach (var button in Inventorylist)
        {
            Destroy(button.gameObject);
        }
        Inventorylist.Clear(); // Clear the list after destroying the buttons
    }

    // Function to filter items based on the search type
    private List<Item> GetFilteredItems(Item Search)
    {
        List<Item> SearchResults = new List<Item>();
        foreach (Item x in PublicItems.InInventory)
        {
            if (x.Type == Search.Type)
            {
                SearchResults.Add(x);
            }
        }
        return SearchResults;
    }

    // Function to create a button for a given item
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

    public void Equipmentchangebuttonpress(ItemType item, Item item1)
    {
        ClearInventoryList();
        ScrollSpace.gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        EventManager.OnItemSelect -= SpawnButtons;
        EventManager.OnEquipmentChange -= Equipmentchangebuttonpress;
    }
}
