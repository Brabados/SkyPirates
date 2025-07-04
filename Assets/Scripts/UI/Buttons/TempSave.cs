using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TempSave : MonoBehaviour
{
    public Map ToSave;
    public InputField input;

    public void Press()
    {
        if (input.text != null || input.text != "" || input.text != " ")
        {
            SaveLoadManager.SaveLoadInstance.SaveMapToJson(ToSave, Application.persistentDataPath + "/" + input.text.ToString() + ".json");
        }
    }
}
