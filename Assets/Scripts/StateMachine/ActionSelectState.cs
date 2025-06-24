using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionSelectState : HexSelectState
{
    private MenuSelect menuSelect;
    private MenuHighlight menuHighlight;
    public override void EnterState(HexSelectManager manager)
    {
        manager.UI.enabled = true;

        // Always pull the last pawn tile, even if something else is highlighted
        Tile fallbackTile = manager.LastPawnTile;

        if (fallbackTile == null)
        {
            Debug.LogError("ActionSelectState: No last pawn tile found.");
            return;
        }

        GameObject select = fallbackTile.gameObject;

        menuSelect = manager.GetComponent<MenuSelect>();
        menuHighlight = manager.GetComponent<MenuHighlight>();

        manager.Responce = menuSelect;
        manager.Highlight = menuHighlight;

        menuSelect.Select(select); // This sets the selection to the fallback pawn tile
        manager.Highlight.SetHighlight(select); // Ensure visual is reset to fallback tile
    }


    public override void ExitState(HexSelectManager manager)
    {
        //disables buttons
        manager.UI.enabled = false;
    }

    public override void UpdateState(HexSelectManager manager)
    {
        // for consideration: still allows mouse hex selection
        //otherwise should remain blank/allow button selection rollover from bottom and top both 
        //are options as unity handles button selection for key board. May have to configure for gamepads.
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
}
