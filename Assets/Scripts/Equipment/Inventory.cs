using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Head,
    Body,
    Feet,
    Accessory,
    Weapon
}

public enum PawnNames
{
    Kai,
    Manahli,
    Adine,
    Urstanor,
    MC,
    Last,
    Terra
}

public class Inventory : MonoBehaviour
{
    public List<Item> InInventory;
    // Start is called before the first frame update

    public void Add(Item toAdd)
    {
        if (!InInventory.Contains(toAdd))
        {
            InInventory.Add(toAdd);
        }
    }

    public void Remove(Item toRemove)
    {
        if (InInventory.Contains(toRemove))
        {
            InInventory.Remove(toRemove);
        }
    }

}
