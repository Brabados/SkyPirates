using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveAbility", menuName = "ScriptableObject/ActiveAbility")]
public class ActiveAbility : BaseAbility
{

    [SerializeField]
    public int ManaCost;

    [SerializeField]
    public List<BaseAction> Actions;


}
