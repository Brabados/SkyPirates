using UnityEngine;

public class InventoryItemButton : MonoBehaviour
{
    public Item Equip;

    public void onClick()
    {
        if (CanvasManager.CanvasInstance.position == 0)
        {
            EventManager.TriggerEquipmentChange(Equip.Type, Equip);
        }
        else if (CanvasManager.CanvasInstance.position == 2)
        {
            EventManager.TriggerShowInfo(Equip);
        }
    }
}
