using System.Collections.Generic;
using UnityEngine;

public class AbilityHighlight : MonoBehaviour, IHighlightResponce
{
    public Material HighlightMat;
    public Material AreaHighlightMat;

    private GameObject highlightSelect = null;
    private Tile highlightTile = null;

    public List<Tile> Area = new List<Tile>();
    private ActiveAbility activeAbility;

    public void SetActiveAbility(ActiveAbility ability)
    {
        activeAbility = ability;
    }

    public void SetHighlight(GameObject highlight)
    {
        if (highlight != null && highlight != highlightSelect)
        {
            Tile tile = highlight.GetComponent<Tile>();
            if (Area.Contains(tile))
            {
                highlightSelect = highlight;
                highlightTile = tile;

                UpdateAbilityPreview();
            }
        }
    }

    public void MoveHighlight(Vector2 input)
    {
        if (highlightTile == null) return;

        Tile check = highlightTile.CheckNeighbours(input);
        if (check != null && Area.Contains(check))
        {
            SetHighlight(check.gameObject);
        }
    }

    public GameObject ReturnHighlight()
    {
        return highlightSelect;
    }

    public void UpdateAbilityPreview()
    {

        if (highlightTile == null || activeAbility == null) return;

        foreach (BaseAction action in activeAbility.Actions)
        {
            List<Tile> tiles = GetTilesForAction(action, HexSelectManager.Instance.LastPawnTile, highlightTile);

           // tiles.RemoveAll(t => !TargetValidator.IsValidTarget(t, action));

            foreach (Tile t in tiles)
            {
                t.Hex.meshupdate(AreaHighlightMat);
            }
        }

        highlightTile.Hex.meshupdate(HighlightMat);
    }

    private List<Tile> GetTilesForAction(BaseAction action, Tile origin, Tile target)
    {
        var finder = HexSelectManager.Instance.HighlightFinder;

        switch (action.Area)
        {
            case EffectArea.Single:
                return new List<Tile> { target };
            case EffectArea.Area:
                return finder.AreaRing(target, action.Size);
            case EffectArea.Ring:
                return finder.HexRing(target, action.Size);
            case EffectArea.Line:
                return finder.AreaLine(origin, target, action.Range);
            case EffectArea.Cone:
                return finder.AreaCone(origin, target, action.Range);
            case EffectArea.Diagonal:
                // Optional: Add diagonal handling if implemented
                return new List<Tile>();
            default:
                return new List<Tile>();
        }
    }

    public void CleanUp()
    {
        highlightSelect = null;
        highlightTile = null;
        Area.Clear();
    }
}
