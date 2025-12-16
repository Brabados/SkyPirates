using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Generates a filtered list of equipment items based on the selected equipment slot type
/// </summary>
public class EquipmentButtonSpawner : ScrollListGenerator<Item>
{
    [Header("Inventory Data")]
    [SerializeField] private Inventory inventory;

    private ItemType currentFilterType;
    private List<Item> filteredItems = new List<Item>();

    void Start()
    {
        autoSelectFirstButton = false; // Don't auto-select spawned items
        EventManager.OnItemSelect += OnItemSelected;
        EventManager.OnEquipmentChange += OnEquipmentChanged;
    }

    protected override List<Item> GetListData() => filteredItems;

    protected override string GetItemDisplayText(Item item) => item.Name;

    protected override void ConfigureButton(UnityEngine.UI.Button button, Item item)
    {
        InventoryItemButton itemButton = button.gameObject.AddComponent<InventoryItemButton>();
        itemButton.Equip = item;
        button.onClick.AddListener(itemButton.onClick);
    }

    private void OnItemSelected(Item selectedItem)
    {
        if (selectedItem == null) return;

        // Clear old buttons before creating new ones
        ClearList();

        currentFilterType = selectedItem.Type;
        FilterItemsByType(currentFilterType);

        // Show the content area
        if (contentRect != null)
        {
            contentRect.gameObject.SetActive(true);
        }

        RefreshList();

        // Select first spawned button manually
        if (spawnedButtons.Count > 0 && spawnedButtons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(spawnedButtons[0].gameObject);
        }
    }

    private void FilterItemsByType(ItemType type)
    {
        filteredItems.Clear();

        if (inventory?.InInventory == null) return;

        foreach (Item item in inventory.InInventory)
        {
            if (item.Type == type)
                filteredItems.Add(item);
        }

        Debug.Log($"[EquipmentButtonSpawner] Filtered {filteredItems.Count} items of type {type}");
    }

    private void OnEquipmentChanged(ItemType itemType, Item item)
    {
        // Clear the spawned buttons
        ClearList();

        // Hide the content area
        if (contentRect != null)
        {
            contentRect.gameObject.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.OnItemSelect -= OnItemSelected;
        EventManager.OnEquipmentChange -= OnEquipmentChanged;
    }
}
