using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExitButton : MonoBehaviour
{
    public void quit()
    {
        Debug.Log("Quit");
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
