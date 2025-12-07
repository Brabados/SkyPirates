using UnityEngine;

public class GenerateFromFile : MonoBehaviour, IGenerate
{
    public string FileName;
    public Board Generate(Map Data)
    {
        Data.PlayArea = SaveLoadManager.LoadBoardFromJson(Application.persistentDataPath + "/" + FileName + ".json", Data, Data.gameObject.transform);
        return Data.PlayArea;
    }
}
