using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveSelectState : HexSelectState
{
    private MoveSelect moveSelect;
    private MovementHighlight moveHighlight;
    private List<Tile> movementRange;

    public override void EnterState(HexSelectManager manager)
    {
        moveSelect = manager.GetComponent<MoveSelect>();
        moveHighlight = manager.GetComponent<MovementHighlight>();

        // Always fallback to the last pawn tile
        Tile hex = manager.LastPawnTile;

        if (hex == null || hex.Contents == null)
        {
            Debug.LogError("MoveSelectState: Selected tile is invalid or has no pawn.");
            return;
        }


        GameObject selectedObject = hex.gameObject;
        Pawn pawn = hex.Contents;

        manager.Responce = moveSelect;
        manager.Highlight = moveHighlight;

        moveSelect.SelectedCharater = pawn;
        moveSelect.Selections.Clear();
        moveSelect.Selections.Add(hex);

        moveSelect.Area = manager.HighlightFinder.HexReachable(hex, pawn.Stats.Movement);
        movementRange = moveSelect.Area;
        moveHighlight.Area = moveSelect.Area;
        moveHighlight.Starthighlight(selectedObject);

        foreach (Tile tile in moveSelect.Area)
        {
            tile.Hex.meshupdate(moveSelect.HighlightMat);
        }

        // Ensure selection stays tracked globally
        manager.SelectedTiles.Add(hex);
    }



    public override void UpdateState(HexSelectManager manager)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            manager.Highlight.SetHighlight(hit.transform.gameObject);
        }
        if (manager.InputActions.Battle.MoveSelection.triggered)
        {
            manager.Highlight.MoveHighlight(manager.InputActions.Battle.MoveSelection.ReadValue<Vector2>());
        }
        if (manager.InputActions.Battle.Select.triggered)
        {
            manager.Select();
        }
        if (manager.InputActions.Battle.Deselect.triggered)
        {
            manager.Responce.Deselect();
        }
    }

    public override void ExitState(HexSelectManager manager)
    {
        if (movementRange != null)
        {
            foreach (Tile tile in movementRange)
            {
                
                tile.Hex.meshupdate(tile.BaseMaterial);
            }
        }

        moveHighlight.CleanUp();
        moveSelect.CleanUP();

        movementRange?.Clear();
    }




}
