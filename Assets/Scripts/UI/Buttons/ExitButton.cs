using UnityEditor;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void quit()
    {
        Debug.Log("Quit");
        Application.Quit();
        EditorApplication.isPlaying = false;
    }
}
