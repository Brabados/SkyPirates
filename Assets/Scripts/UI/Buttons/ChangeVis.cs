using UnityEngine;

public class ChangeVis : MonoBehaviour
{
    public GameObject Vis;

    public void ChangeActive()
    {
        if (!Vis.activeInHierarchy)
        {
            Vis.SetActive(true);
        }
        else
        {
            Vis.SetActive(false);
        }
    }
}
