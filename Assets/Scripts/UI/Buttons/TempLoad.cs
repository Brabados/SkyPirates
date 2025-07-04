using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class TempLoad : MonoBehaviour
{
    public string Load;

    public Map ToLoad;


    public void Press()
    {
        for (int x = 0; x < ToLoad.PlayArea._size_X; x++)
        {
            for (int y = 0; y < ToLoad.PlayArea._size_Y; y++)
            {
                Destroy(ToLoad.PlayArea.get_Tile(x, y).gameObject);
            }
        }
        ToLoad.PlayArea = SaveLoadManager.LoadBoardFromJson(Application.persistentDataPath + "/" + Load, ToLoad, ToLoad.gameObject.transform);
        ToLoad.SetNeighbours(ToLoad.PlayArea, ToLoad.isFlatTopped);
        ToLoad.setFirstHex();
        Debug.Log("Map loaded from JSON.");
        Debug.Log(Application.persistentDataPath + "/" + Load);
    }
}
