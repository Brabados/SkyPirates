using UnityEngine;
using UnityEngine.InputSystem;

public class EditState : HexSelectState
{
    private BuildHighlight highlightState;
    private BuildSelect selectState;

    public override void EnterState(HexSelectManager manager)
    {
        highlightState = manager.GetComponent<BuildHighlight>();
        selectState = manager.GetComponent<BuildSelect>();

        manager.Highlight = highlightState;
        manager.Responce = selectState;

        selectState.Deselect();
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
            Vector2 move = manager.InputActions.Battle.MoveSelection.ReadValue<Vector2>();
            manager.Highlight.MoveHighlight(move);
        }

        if (manager.InputActions.Battle.Select.triggered)
        {
            GameObject tileObj = manager.Highlight.ReturnHighlight();
            if (tileObj != null)
            {
                manager.Responce.Select(tileObj);
            }
        }

        if (manager.InputActions.Battle.Deselect.triggered)
        {
            manager.Responce.Deselect();
        }
    }

    public override void ExitState(HexSelectManager manager)
    {
        // Optional: Clean-up or save actions before leaving the editor state
    }
}
