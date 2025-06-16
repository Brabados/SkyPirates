using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Commits the selected tile edit
public class BuildSelect : MonoBehaviour, ISelectionResponce
{
    public Material selectedEditMaterial; // Material to apply on select

    private TileDataSO EditTile;

    private GameObject selectedObject = null;
    private Tile selectedTile = null;
    private int tilehight = 1;

    public void Start()
    {
        EventManager.OnTileChange += ChangeTile;
        EventManager.OnSliderValueChange += ChangeHight;
    }

    public void ChangeHight(float hight)
    {
        tilehight = (int)hight;
    }

    public void Select(GameObject selection)
    {
        if (selection != null)
        {
            selectedObject = selection;
            selectedTile = selectedObject.GetComponent<Tile>();

            if (selectedTile != null)
            {
                Map playarea = FindObjectOfType<Map>();


                GameObject holder = new GameObject($"Hex {selectedTile.Column},{selectedTile.Row}", typeof(Tile));
                Tile tile = holder.GetComponent<Tile>();
                tile.Data = EditTile;
                tile.SetPositionAndHeight(
                    new Vector2Int(selectedTile.Column, selectedTile.Row), 
                    selectedTile.ReturnSquareCoOrds(), tile.Data.Name == "Air" ? 5 : tilehight * 5);
               
                Vector3 tilePosition = playarea.GetHexPositionFromCoordinate(new Vector2Int(selectedTile.Column,selectedTile.Row ));
                tilePosition.y = tilePosition.y + tile.Height / 2;
                holder.transform.position = tilePosition;
                holder.transform.SetParent(playarea.transform);
                Instantiate(tile.Data.TilePrefab, holder.transform).transform.position += new Vector3(0, tile.Height / 2 - 1, 0);
                tile.SetupHexRenderer(playarea.innerSize, playarea.outerSize, playarea.isFlatTopped);
                tile.SetPosition(new Vector2Int(selectedTile.Column, selectedTile.Row));
                tile.SetPawnPos();
                playarea.PlayArea.set_Tile(selectedTile.Column, selectedTile.Row, tile);
                playarea.setSingleNeighbour(selectedTile.Column, selectedTile.Row);
                Destroy(selectedTile.gameObject);
            }
        }
    }

    public void Deselect()
    {
        selectedObject = null;
        selectedTile = null;
    }

    public GameObject CurrentSelection()
    {
        return selectedObject;
    }

    public void ChangeTile(TileDataSO tile)
    {
        EditTile = tile;
    }

    public void OnDestroy()
    {
        EventManager.OnTileChange -= ChangeTile;
        EventManager.OnSliderValueChange -= ChangeHight;
    }
}
