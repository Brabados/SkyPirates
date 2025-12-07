using UnityEngine;
using UnityEngine.InputSystem;
public class DefaultSelectState : HexSelectState
{
    private HexSelection HexState;

    public override void EnterState(HexSelectManager manager)
    {
        if (TurnManager.Instance.currentTurn != null)
        {
            TurnManager.Instance.currentTurn.MovementLeft = TurnManager.Instance.currentTurn.FullMovement;
            TurnManager.Instance.currentTurn.ActionTaken = false;
        }
        HexState = manager.GetComponent<HexSelection>();
        manager.Responce = HexState;
        manager.Highlight = manager.GetComponent<HexHighlight>();
        if (CanvasManager.CanvasInstance != null)
        {
            EventManager.TriggerHideCanvas(((int)Menues.PawnInfo));
        }
    }

    public override void UpdateState(HexSelectManager manager)
    {
        Pawn st = null;
        int count = 0;
        while (st == null)
        {
            st = TurnManager.Instance.UpdateTurn();
            count++;
            if (count > 300)
            {
                st = GameObject.FindObjectOfType<Pawn>();
            }
        }
        if (st is PlayerPawns)
        {
            manager.Highlight.SetHighlight(st.gameObject);
            manager.Select();
        }
        else if (st is EnemyPawn)
        {
            manager.SwitchToEnemyTurnState();
        }
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
