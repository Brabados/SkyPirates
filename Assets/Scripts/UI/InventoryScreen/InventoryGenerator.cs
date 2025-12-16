using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a scrollable list of all items in the player's inventory
/// </summary>
public class InventoryGenerator : ScrollListGenerator<Item>
{
    [Header("Inventory Data")]
    [SerializeField] private Inventory inventory;

    void Start()
    {
        autoSelectFirstButton = true;
        RefreshList();
    }

    protected override List<Item> GetListData()
    {
        if (inventory == null)
        {
            Debug.LogError("[InventoryGenerator] Inventory is NULL!");
            return new List<Item>();
        }

        if (inventory.InInventory == null)
        {
            Debug.LogWarning("[InventoryGenerator] Inventory.InInventory is NULL!");
            return new List<Item>();
        }

        return inventory.InInventory;
    }

    protected override string GetItemDisplayText(Item item) => item.Name;

    protected override void ConfigureButton(UnityEngine.UI.Button button, Item item)
    {
        InventoryItemButton itemButton = button.gameObject.AddComponent<InventoryItemButton>();
        itemButton.Equip = item;
        button.onClick.AddListener(itemButton.onClick);
    }
}
