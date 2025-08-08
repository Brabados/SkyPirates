using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveButton : MonoBehaviour
{
    public Button AttachedButton;
    public void Move()
    {
        HexSelectManager.Instance.SwitchToMoveSelectState();           
    }
    public void Start()
    {
        EventManager.OnMovementAllUsed += Disable;
    }

    public void Disable(bool state)
    {
        AttachedButton.enabled = state;
    }

    public void OnDestroy()
    {
        EventManager.OnMovementAllUsed -= Disable;
    }
}
