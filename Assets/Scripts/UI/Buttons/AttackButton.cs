using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{

    public ActiveAbility BasicAttack;

    public void Attack()
    {
        HexSelectManager.Instance.SwitchToAbilityState(BasicAttack);
    }

}
