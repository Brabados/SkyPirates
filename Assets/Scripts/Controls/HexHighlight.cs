using UnityEngine;

//Base highlight implementation.
public class HexHighlight : MonoBehaviour, IHighlightResponce
{
    private GameObject HighLightSelect = null;
    private Tile HighlightTile = null;
    public Material HighlightMat;
    private Pawn HighlightContent;

    //Sets the Highlight
    public void SetHighlight(GameObject Input)
    {
        if (HighLightSelect != Input)
        {
            if (HighLightSelect != null && HighlightTile != null)
            {
                // Only unhighlight if not selected
                if (!HexSelectManager.Instance.SelectedTiles.Contains(HighlightTile))
                {
                    HighlightTile.Hex.meshupdate(HighlightTile.BaseMaterial);
                }
            }

            HighLightSelect = Input;
            HighlightTile = HighLightSelect.GetComponent<Tile>();

            if (HighlightTile == null)
            {
                HighlightTile = HighLightSelect.GetComponent<Pawn>()?.Position;
                if (HighlightTile != null)
                    HighLightSelect = HighlightTile.gameObject;
            }

            HighlightContent = HighlightTile.Contents;

            // Only apply highlight if tile is not selected
            if (!HexSelectManager.Instance.SelectedTiles.Contains(HighlightTile))
            {
                HighlightTile.Hex.meshupdate(HighlightMat);
            }
        }
    }



    //Finds the Tile in the direction relitive to the camera and moves the highlight 1 space.
    public void MoveHighlight(Vector2 Input)
    {
        Tile check = HighlightTile.CheckNeighbours(Input);
        if (check != null)
        {
            SetHighlight(check.gameObject);
        }
    }

    //Returns the current highlight
    public GameObject ReturnHighlight()
    {
        return HighLightSelect;
    }

}
