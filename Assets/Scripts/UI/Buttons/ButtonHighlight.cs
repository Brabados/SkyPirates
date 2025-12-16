using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles button hover and selection events to display item information
/// </summary>
public class ButtonHighlight : Button, IPointerEnterHandler, ISelectHandler 
{
    public override void OnPointerExit(PointerEventData eventData)
    {
        EventManager.TriggerInfoReset();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        GameObject hoveredObject = eventData.pointerEnter;

        ItemButton itemButton = hoveredObject.GetComponentInParent<ItemButton>();
        if (itemButton?.CurrentEquip != null)
        {
            EventManager.TriggerInfoChange(itemButton.CurrentEquip);
            return;
        }

        InventoryItemButton inventoryButton = hoveredObject.GetComponentInParent<InventoryItemButton>();
        if (inventoryButton?.Equip != null)
        {
            EventManager.TriggerInfoCompare(inventoryButton.Equip);
            return;
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        GameObject selectedObject = eventData.selectedObject;

        EventManager.TriggerInfoReset();

        ItemButton itemButton = selectedObject.GetComponent<ItemButton>();
        if (itemButton?.CurrentEquip != null)
        {
            EventManager.TriggerInfoChange(itemButton.CurrentEquip);
            return;
        }

        InventoryItemButton inventoryButton = selectedObject.GetComponent<InventoryItemButton>();
        if (inventoryButton?.Equip != null)
        {
            EventManager.TriggerInfoCompare(inventoryButton.Equip);
            return;
        }
    }
}
