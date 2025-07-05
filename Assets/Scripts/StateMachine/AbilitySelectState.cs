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

        // Calculate ability range
        abilitySelect.Area = GetAbilityRange(selectedTile, Active);
        abilityHighlight.Area = abilitySelect.Area;

        abilityRange = abilitySelect.Area;
        if(abilityRange.Count == 0)
        {
            Debug.Log("No vaild Target in range");
            HexSelectManager.Instance.ReturnToPreviousState();
            return;
        }

        // Paint valid selection tiles
        foreach (Tile tile in abilityRange)
        {
            tile.Hex.meshupdate(abilitySelect.HighlightMat);
        }

        // Set starting highlight
        //abilityHighlight.SetHighlight(current);
    }

    private List<Tile> GetAbilityRange(Tile start, ActiveAbility ability)
    {
        List<Tile> range = new List<Tile>();

        foreach (BaseAction action in ability.Actions)
        {
            RangeFinder finder = HexSelectManager.Instance.HighlightFinder;

            List<Tile> actionRange = new List<Tile>();
            List<Tile> invalidTiles = new List<Tile>();

            actionRange = finder.AreaRing(start, action.Range);

            switch(action.Targettype)
            {
                case Target.Self:
                    foreach(Tile t in actionRange)
                    {
                        if(!HexSelectManager.Instance.SelectedTiles.Contains(t))
                        {
                            invalidTiles.Add(t);
                        }
                    }
                    break;
                case Target.Pawn:
                    foreach (Tile t in actionRange)
                    {
                        if (!(t.Contents is PlayerPawns || t.Contents is EnemyPawn) || HexSelectManager.Instance.SelectedTiles.Contains(t))
                        {
                            invalidTiles.Add(t);
                        }
                    }
                    break;
                case Target.Enemy:
                    foreach (Tile t in actionRange)
                    {
                        if (!(t.Contents is EnemyPawn))
                        {
                            invalidTiles.Add(t);
                        }
                        else if (HexSelectManager.Instance.SelectedTiles.Contains(t))
                        {
                            invalidTiles.Add(t);
                        }
                    }
                    break;
                case Target.Friendly:
                    foreach (Tile t in actionRange)
                    {
                        if (!(t.Contents is PlayerPawns) || HexSelectManager.Instance.SelectedTiles.Contains(t))
                        {
                            invalidTiles.Add(t);
                        }
                    }
                    break;
            }

            foreach(Tile t in invalidTiles)
            {
                actionRange.Remove(t);
            }

            foreach (Tile t in actionRange)
            {
                if (!range.Contains(t))
                {
                    range.Add(t);
                }
            }
        }

        return range;
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
