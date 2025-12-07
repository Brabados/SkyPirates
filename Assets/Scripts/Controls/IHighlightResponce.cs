using UnityEngine;
//Interface class to allow iteration on diffrent highlight needs.
public interface IHighlightResponce
{
    public void SetHighlight(GameObject Highlight);
    public void MoveHighlight(Vector2 Input);
    public GameObject ReturnHighlight();
}
