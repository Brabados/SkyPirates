using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitySelectState : HexSelectState
{
    private MenuSelect menuSelect;
    private AbilityHighlight menuHighlight;

    public override void EnterState(HexSelectManager manager)
    {
        GameObject current = manager.Responce.CurrentSelection();
        manager.UI.enabled = true;
        menuSelect = manager.GetComponent<MenuSelect>();
        menuSelect.Select(current);
        menuHighlight = manager.GetComponent<AbilityHighlight>();
        manager.Responce = menuSelect;
        manager.Highlight = menuHighlight;
    }

    public override void ExitState(HexSelectManager manager)
    {
        manager.UI.enabled = false;
        if (menuHighlight != null)
        {
            menuHighlight.ClearHighlights();
        }
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
}
