using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexSelection : MonoBehaviour, ISelectionResponce
{
    public Material selectedMat;
    public GameObject SelectedObject { get; private set; } = null;
    public Tile SelectedTile { get; private set; } = null;
    public Pawn SelectedContents { get; private set; } = null;

    public List<Tile> movementRangeEnemy;

    public void Select(GameObject selection)
    {
        if (selection == null)
        {
            Debug.LogWarning("[HexSelection] Tried to select a null object.");
            return;
        }

        Tile selectedTile = selection.GetComponent<Tile>();
        if (selectedTile == null)
        {
            Debug.LogWarning("[HexSelection] Selected object is not a tile.");
            return;
        }

        // If selecting a different tile, clean up the previous selection
        if (SelectedObject != null && SelectedTile != selectedTile)
        {
            Deselect();
        }

        //  Update local selection
        SelectedObject = selection;
        SelectedTile = selectedTile;
        SelectedContents = SelectedTile.Contents;

        //  Track selection globally
        HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);

        //  If this tile has a pawn, update the last pawn tile
        if (SelectedContents != null)
        {
            if (SelectedContents is PlayerPawns)
            {
                HexSelectManager.Instance.UpdateLastPawnTile(SelectedTile);
                EventManager.TriggerPawnSelect(SelectedContents);
                HexSelectManager.Instance.SwitchToActionSelectState();
            }
            else
            {
                movementRangeEnemy = HexSelectManager.Instance.HighlightFinder.AreaRing(SelectedContents.Position, SelectedContents.Stats.Movement);

                // Clean movement list to remove invalid tiles
                for (int i = movementRangeEnemy.Count - 1; i >= 0; i--)
                {
                    if (movementRangeEnemy[i].Data.MovementCost == 0)
                    {
                        movementRangeEnemy.RemoveAt(i);
                    }
                }
            }
        }

        // Apply selection visual
        SelectedTile.Hex.meshupdate(selectedMat);
    }

    public void Deselect()
    {
        if (SelectedObject != null)
        {
            if (movementRangeEnemy != null)
            {
                foreach (Tile tile in movementRangeEnemy)
                {
                    tile.Hex.meshupdate(tile.BaseMaterial);
                }
                movementRangeEnemy.Clear();
            }

            HexSelectManager.Instance.SelectedTiles.Remove(SelectedTile);

            SelectedTile.Hex.meshupdate(SelectedTile.BaseMaterial);

            SelectedTile = null;
            SelectedContents = null;
            SelectedObject = null;
        }
    }

    public GameObject CurrentSelection()
    {
        return SelectedObject;
    }
}
