using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ItemButton : MonoBehaviour
{

    public ScrollRect ItemDysplay;
    public ItemType SearchItem;
    public Item CurrentEquip;

    public void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {

        }
    }

    public void buttonPress()
    {
        ItemDysplay.gameObject.SetActive(true);
        EventManager.TriggerItemSelect(CurrentEquip);
    }


}
