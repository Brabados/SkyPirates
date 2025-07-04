using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveButton : MonoBehaviour
{

    public void Move()
    {
        HexSelectManager.Instance.SwitchToMoveSelectState();           
    }
}
