using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/WeaponItem")]

public class WeaponItem : Item
{
    public ActiveAbility BaseAttack;
    public void Awake()
    {
        Type = ItemType.Weapon;
    }
}
