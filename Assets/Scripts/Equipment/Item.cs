using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/Item")]
public class Item : BaseScriptableObject
{
    public ItemType Type;
    public string Name;
    public string Info;
    public int[] StatChanges = new int[6];
    public List<BaseAbility> ExtraAbilies;
    public List<PawnNames> WhoCanEquip;

    public override bool Equals(object other)
    {
        if (!(other is Item) || other == null)
        {
            return false;
        }
        else
        {
            if (this.UniqueID == ((Item)other).UniqueID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public override int GetHashCode()
    {
        return this.Type.GetHashCode();
    }
}
