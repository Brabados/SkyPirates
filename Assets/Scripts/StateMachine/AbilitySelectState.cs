using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        //Use new TargetArea to determine selectable area
        abilitySelect.Area = GetTargetableTiles(selectedTile, Active);
        abilityHighlight.Area = abilitySelect.Area;

        abilityRange = abilitySelect.Area;

        if (abilityRange.Count == 0)
        {
            Debug.Log("No valid targets in range.");
            HexSelectManager.Instance.ReturnToPreviousState();
            return;
        }

        // Highlight valid tiles
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

            // Use TargetArea instead of Area
            switch (action.TargetArea)
            {
                case EffectArea.Single:
                    targetArea.Add(start);
                    break;
                case EffectArea.Area:
                    targetArea = finder.AreaRing(start, action.Range);
                    break;
                case EffectArea.Ring:
                    targetArea = finder.HexRing(start, action.Range);
                    break;
                case EffectArea.Line:
                    targetArea = finder.AreaLineFan(start, action.Range);
                    break;
                case EffectArea.Cone:
                    targetArea = finder.AreaConeFan(start, action.Range, action.Size);
                    break;
                case EffectArea.Diagonal:
                    targetArea = finder.AreaDiagonal(start, action.Range);
                    break;
                case EffectArea.Path:
                    targetArea = finder.AreaRing(start, action.Range);
                    break;
            }

            // Filter targets by target type
            foreach (Tile t in targetArea)
            {
                switch (action.Targettype)
                {
                    case Target.Self:
                        if (!HexSelectManager.Instance.SelectedTiles.Contains(t))
                            invalids.Add(t);
                        break;
                    case Target.Tile:
                        break; // any tile valid
                    case Target.Pawn:
                        if (!(t.Contents is PlayerPawns || t.Contents is EnemyPawn) || HexSelectManager.Instance.SelectedTiles.Contains(t))
                            invalids.Add(t);
                        break;
                    case Target.Enemy:
                        if (!(t.Contents is EnemyPawn) || HexSelectManager.Instance.SelectedTiles.Contains(t))
                            invalids.Add(t);
                        break;
                    case Target.Friendly:
                        if (!(t.Contents is PlayerPawns) || HexSelectManager.Instance.SelectedTiles.Contains(t))
                            invalids.Add(t);
                        break;
                }
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
