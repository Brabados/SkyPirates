using UnityEngine;

//Interface class to allow iteration on diffrent selection needs.
public interface ISelectionResponce
{
    public void Select(GameObject Selection);
    public void Deselect();

    public GameObject CurrentSelection();
}
