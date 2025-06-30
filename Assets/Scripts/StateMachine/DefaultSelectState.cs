using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class DefaultSelectState : HexSelectState
{
    private HexSelection HexState;

    public override void EnterState(HexSelectManager manager)
    {
        HexState = manager.GetComponent<HexSelection>();
        manager.Responce = HexState;
        manager.Highlight = manager.GetComponent<HexHighlight>();
        if(CanvasManager.CanvasInstance != null)
        {
            EventManager.TriggerHideCanvas(((int)Menues.PawnInfo));
        }
    }

    public override void UpdateState(HexSelectManager manager)
    {
        // Default update logic from HexSelectManager
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
        // Clean up if necessary
    }
}
