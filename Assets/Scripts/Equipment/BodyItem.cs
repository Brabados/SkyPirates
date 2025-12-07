using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/BodyItem")]

public class BodyItem : Item
{
    // Start is called before the first frame update
    public void Awake()
    {
        Type = ItemType.Body;
    }
}
