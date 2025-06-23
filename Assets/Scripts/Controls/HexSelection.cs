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

    public void Select(GameObject Selection)
    {
        if (Selection != null && SelectedObject == null)
        {
            // Select new object
            SelectedObject = Selection;
            SelectedTile = SelectedObject.GetComponent<Tile>();
            SelectedContents = SelectedTile.Contents;

            //  Add to global selection tracker
            HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);

            if (SelectedContents != null)
            {
                EventManager.TriggerPawnSelect(SelectedContents);
                if (SelectedContents is PlayerPawns)
                {
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

            SelectedTile.Hex.meshupdate(selectedMat);
        }
        else if (Selection != null)
        {
            // If selecting a new tile while one is already selected
            Deselect();
            Select(Selection);
        }
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

            //  Remove from global selection tracker
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
