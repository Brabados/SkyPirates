using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Target
{
    Self,
    Tile,
    Enemy,
    Friendly,
    Pawn
}

public enum EffectArea
{
    Single,
    Area,
    Ring,
    Diagonal,
    Line,
    Cone,
    Path
}

public enum Effect
{
    None,
    Push,
    Pull,
    Slide
}

public enum Affect
{
    None,
    Blind,
    Poison,
    Disease,
    Heavy,
    Levitate,
    Berserk,
    Frozen,
    Pinned,
    Burn,
    Dehydrated,
    Nauseated,
    Stunned,
    Bleed,
    Chilled,
    Focused,
    Concussed,
    Silenced,
    Petrified,
    Deafened,
    Overwhelmed
}


[CreateAssetMenu(fileName = "BaseAction", menuName = "ScriptableObject/BaseAction")]
public class BaseAction : BaseScriptableObject
{
    [SerializeField]
    public Target Targettype;

    [SerializeField]
    public EffectArea TargetArea;

    [SerializeField]
    public EffectArea Area;

    [SerializeField]
    public int Size;

    [SerializeField]
    public int Range;

    [SerializeField]
    public Effect MoveEffect;

    [SerializeField]
    public Affect StatusAffect;

    [SerializeField]
    public int Damage;

    [SerializeField]
    public DamageType DmgType;
}
