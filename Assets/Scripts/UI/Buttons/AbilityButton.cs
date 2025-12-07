using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public Button AttachedButton;
    public void Start()
    {
        EventManager.OnActionExicuted += Disable;
    }

    public void Disable(bool state)
    {
        AttachedButton.enabled = state;
    }

    public void OnDestroy()
    {
        EventManager.OnActionExicuted -= Disable;
    }

}
