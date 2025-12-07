using UnityEngine;

// Highlights a tile with the currently selected edit preview (not permanent)
public class BuildHighlight : MonoBehaviour, IHighlightResponce
{
    private GameObject highlightObject = null;
    private Tile highlightTile = null;

    private TileDataSO EditTile;

    public Material previewMaterial; // Material shown during highlight
    private Material originalMaterial;

    public void Start()
    {
        EventManager.OnTileChange += ChangeTile;
    }
    public void SetHighlight(GameObject input)
    {
        if (highlightObject != input)
        {
            // Restore previous tile
            if (highlightTile != null)
            {
                highlightTile.Hex.meshupdate(originalMaterial);
            }

            highlightObject = input;
            highlightTile = highlightObject.GetComponent<Tile>();

            if (highlightTile != null)
            {
                originalMaterial = highlightTile.Hex.H_Mat;
                highlightTile.Hex.meshupdate(previewMaterial);
            }
        }
    }

    public void MoveHighlight(Vector2 direction)
    {
        if (highlightTile == null) return;
        Tile next = highlightTile.CheckNeighbours(direction);
        if (next != null)
        {
            SetHighlight(next.gameObject);
        }
    }

    public GameObject ReturnHighlight()
    {
        return highlightObject;
    }

    public void ChangeTile(TileDataSO tile)
    {
        EditTile = tile;
        previewMaterial = tile.BaseMat;
    }

    public void OnDestroy()
    {
        EventManager.OnTileChange -= ChangeTile;
    }
}
