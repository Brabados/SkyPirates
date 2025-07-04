using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


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
