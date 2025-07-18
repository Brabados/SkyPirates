using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackButton : MonoBehaviour
{

    public Button AttachedButton;
    public ActiveAbility BasicAttack;

    public void Start()
    {
        EventManager.OnActionExicuted += Disable;
    }

    public void Attack()
    {
        HexSelectManager.Instance.SwitchToAbilityState(BasicAttack);
    }

    public void Disable()
    {
        AttachedButton.enabled = !AttachedButton.enabled;
    }

    public void OnDestroy()
    {
        EventManager.OnActionExicuted -= Disable;
    }

}
