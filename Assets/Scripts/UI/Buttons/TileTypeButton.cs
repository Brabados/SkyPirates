using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTypeButton: MonoBehaviour
{
    public TileDataSO tile;

    public void SetChange()
    {
        EventManager.TriggerTileChange(tile);
    }
}
