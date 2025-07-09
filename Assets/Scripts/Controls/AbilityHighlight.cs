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

    // Track each tile's previous material
    private Dictionary<Tile, Material> previousMaterials = new Dictionary<Tile, Material>();

    public void SetActiveAbility(ActiveAbility ability)
    {
        activeAbility = ability;
    }

    public void SetHighlight(GameObject highlight)
    {
        if (highlight == null || highlight == highlightSelect)
            return;

        Tile tile = highlight.GetComponent<Tile>();

        if (tile == null)
        {
            tile = highlight.GetComponent<Pawn>()?.Position;
            if (tile != null)
                highlight = tile.gameObject;
        }

        if (!Area.Contains(tile))
            return;

        // Unhighlight the previous area
        if (highlightTile != null)
        {
            List<Tile> previousTiles = GetTilesForAction(activeAbility.Actions[0], HexSelectManager.Instance.LastPawnTile, highlightTile);

            foreach (Tile t in previousTiles)
            {
                // Restore the tile's original material if we saved it
                if (previousMaterials.TryGetValue(t, out Material originalMat))
                {
                    t.Hex.meshupdate(originalMat);
                }
            }

            previousMaterials.Clear();

            // Unhighlight previous highlight tile if it's not selected
            if (!HexSelectManager.Instance.SelectedTiles.Contains(highlightTile))
            {
                if (previousMaterials.TryGetValue(highlightTile, out Material originalMat))
                {
                    highlightTile.Hex.meshupdate(originalMat);
                }
            }
        }

        // Update to new highlight
        highlightSelect = highlight;
        highlightTile = tile;

        // Apply highlight to the new tile if it's not selected
        if (!HexSelectManager.Instance.SelectedTiles.Contains(highlightTile))
        {
            // Save current material before changing
            previousMaterials[highlightTile] = highlightTile.Hex.currentMat();
            highlightTile.Hex.meshupdate(HighlightMat);
        }

        // Highlight the new ability preview area
        UpdateAbilityPreview();
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

            foreach (Tile t in tiles)
            {
                // Save the current material before overwriting it
                if (!previousMaterials.ContainsKey(t))
                {
                    previousMaterials[t] = t.Hex.currentMat();
                }

                t.Hex.meshupdate(AreaHighlightMat);
            }
        }

        // Always make sure the highlight tile has the highlight material
        if (!HexSelectManager.Instance.SelectedTiles.Contains(highlightTile))
        {
            highlightTile.Hex.meshupdate(HighlightMat);
        }
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
                return finder.AreaCone(origin, target, action.Range, action.Size);
            case EffectArea.Diagonal:
                return finder.AreaDiagonal(target, action.Range);
            case EffectArea.Path:
                return finder.AreaPath(origin, target, action.Range);
            default:
                return new List<Tile>();
        }
    }

    public void CleanUp()
    {
        // Reset all previously highlighted tiles to their saved materials
        foreach (var kvp in previousMaterials)
        {
            kvp.Key.Hex.meshupdate(kvp.Value);
        }

        previousMaterials.Clear();
        highlightSelect = null;
        highlightTile = null;
        Area.Clear();
    }
}
