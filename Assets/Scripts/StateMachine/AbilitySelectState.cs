using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// --- AbilitySelectState.cs ---

public class AbilitySelectState : HexSelectState
{
    private AbilitySelect abilitySelect;
    private AbilityHighlight abilityHighlight;
    public ActiveAbility Active;

    private List<Tile> abilityRange;

    public override void EnterState(HexSelectManager manager)
    {
        Tile selectedTile = manager.GetCurrentSelectedTile();
        if (selectedTile == null || selectedTile.Contents == null)
        {
            Debug.LogError("AbilitySelectState: No valid pawn selected.");
            return;
        }

        GameObject current = selectedTile.gameObject;

        abilitySelect = manager.GetComponent<AbilitySelect>();
        abilityHighlight = manager.GetComponent<AbilityHighlight>();

        manager.Responce = abilitySelect;
        manager.Highlight = abilityHighlight;

        abilitySelect.ActiveAbility = Active;
        abilityHighlight.SetActiveAbility(Active);

        abilitySelect.Area = GetTargetableTiles(selectedTile, Active);
        abilityHighlight.Area = abilitySelect.Area;

        abilityRange = abilitySelect.Area;

        if (abilityRange.Count == 0)
        {
            Debug.Log("No valid targets in range.");
            HexSelectManager.Instance.ReturnToPreviousState();
            return;
        }

        foreach (Tile tile in abilityRange)
        {
            tile.Hex.meshupdate(abilitySelect.HighlightMat);
        }
    }

    private List<Tile> GetTargetableTiles(Tile start, ActiveAbility ability)
    {
        List<Tile> result = new List<Tile>();

        foreach (BaseAction action in ability.Actions)
        {
            RangeFinder finder = HexSelectManager.Instance.HighlightFinder;

            List<Tile> targetArea = new List<Tile>();
            List<Tile> invalids = new List<Tile>();

            int size = action.TargetSize;

            switch (action.TargetArea)
            {
                case EffectArea.Single:
                    targetArea.Add(start);
                    break;
                case EffectArea.Area:
                    targetArea = finder.AreaRing(start, size);
                    break;
                case EffectArea.Ring:
                    targetArea = finder.HexRing(start, size);
                    break;
                case EffectArea.Line:
                    targetArea = finder.AreaLineFan(start, size);
                    break;
                case EffectArea.Cone:
                    targetArea = finder.AreaConeFan(start, size, action.Size);
                    break;
                case EffectArea.Diagonal:
                    targetArea = finder.AreaDiagonal(start, size);
                    break;
                case EffectArea.Path:
                    // Approximation: allow targeting in AreaRing for lack of better path system
                    targetArea = finder.AreaRing(start, size);
                    break;
            }

            foreach (Tile t in targetArea)
            {
                if (!IsValidTarget(action, t))
                    invalids.Add(t);
            }

            foreach (Tile t in invalids)
            {
                targetArea.Remove(t);
            }

            foreach (Tile t in targetArea)
            {
                if (!result.Contains(t))
                    result.Add(t);
            }
        }

        return result;
    }

    private bool IsValidTarget(BaseAction action, Tile t)
    {
        switch (action.Targettype)
        {
            case Target.Self:
                return HexSelectManager.Instance.SelectedTiles.Contains(t);
            case Target.Tile:
                return true;
            case Target.Pawn:
                return t.Contents is PlayerPawns || t.Contents is EnemyPawn;
            case Target.Enemy:
                return t.Contents is EnemyPawn;
            case Target.Friendly:
                return t.Contents is PlayerPawns;
            default:
                return false;
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

    public override void ExitState(HexSelectManager manager)
    {
        if (abilityRange != null)
        {
            foreach (Tile tile in abilityRange)
            {
                tile.Hex.meshupdate(tile.BaseMaterial);
            }
        }

        abilityHighlight.CleanUp();
        abilitySelect.CleanUp();

        abilityRange?.Clear();
    }
}


