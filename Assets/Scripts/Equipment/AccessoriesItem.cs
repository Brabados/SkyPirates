using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/AccessoriesItem")]

public class AccessoriesItem : Item
{
    // Start is called before the first frame update
    public void Awake()
    {
        Type = ItemType.Accessory;
    }
}
