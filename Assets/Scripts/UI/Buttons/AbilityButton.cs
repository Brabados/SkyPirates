using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public Button AttachedButton;
    public void Start()
    {
        EventManager.OnActionExicuted += Disable;
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
