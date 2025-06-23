using System.Collections.Generic;
using UnityEngine;

public class AbilityHighlight : MonoBehaviour, IHighlightResponce
{
    public Material HighlightMat;
    public Material AreaHighlightMat;

    private GameObject _highlightedObject = null;
    private Tile _highlightedTile = null;
    private Pawn _highlightedContent = null;

    private RangeFinder _rangeFinder;
    private Map _map;

    void Start()
    {
        _rangeFinder = FindObjectOfType<RangeFinder>();
        _map = FindObjectOfType<Map>();
    }

    public void SetHighlight(GameObject input)
    {
        if (_highlightedObject != input)
        {
            if (_highlightedObject != null && _highlightedTile != null)
            {
                if (!HexSelectManager.Instance.SelectedTiles.Contains(_highlightedTile))
                {
                    _highlightedTile.Hex.meshupdate(_highlightedTile.BaseMaterial);
                }
            }

            _highlightedObject = input;
            _highlightedTile = _highlightedObject.GetComponent<Tile>();

            if (_highlightedTile == null)
            {
                Pawn maybePawn = _highlightedObject.GetComponent<Pawn>();
                if (maybePawn != null)
                {
                    _highlightedTile = maybePawn.Position;
                    _highlightedObject = _highlightedTile.gameObject;
                }
            }

            if (_highlightedTile != null)
            {
                _highlightedContent = _highlightedTile.Contents;
                _highlightedTile.Hex.meshupdate(HighlightMat);
            }
        }
    }

    public void MoveHighlight(Vector2 input)
    {
        if (_highlightedTile == null) return;

        Tile check = _highlightedTile.CheckNeighbours(input);
        if (check != null)
        {
            SetHighlight(check.gameObject);
        }
    }

    public GameObject ReturnHighlight()
    {
        return _highlightedObject;
    }


    public void HighlightAbility(ActiveAbility ability, int direction = 0)
    {
        if (_highlightedTile == null) return;

        ClearHighlights();

        foreach (BaseAction action in ability.Actions)
        {
            List<Tile> tiles = GetTilesForAction(action, _highlightedTile, direction);
            foreach (Tile t in tiles)
            {
                if (t != null && t.Hex != null)
                    t.Hex.meshupdate(AreaHighlightMat);
            }
        }

        // Re-apply selected tile highlight
        if (_highlightedTile != null)
            _highlightedTile.Hex.meshupdate(HighlightMat);
    }



    private List<Tile> GetTilesForAction(BaseAction action, Tile origin, int direction)
    {
        switch (action.Area)
        {
            case EffectArea.Single:
                return new List<Tile> { origin };

            case EffectArea.Area:
                return _rangeFinder.AreaRing(origin, action.Size);

            case EffectArea.Ring:
                return _rangeFinder.HexRing(origin, action.Size);

            case EffectArea.Line:
                return _rangeFinder.AreaLine(origin, _highlightedTile, direction);

            case EffectArea.Cone:
                return _rangeFinder.AreaCone(origin, action.Range, direction);

            case EffectArea.Diagonal:
                return new List<Tile>(); // Placeholder
        }

        return new List<Tile>();
    }

    public void ClearHighlights()
    {
        foreach (Tile t in _map.PlayArea.GetAllTiles())
        {
            if (t != null && t.Hex != null)
                t.Hex.meshupdate(t.BaseMaterial);
        }

        // Re-apply highlight
        if (_highlightedTile != null)
            _highlightedTile.Hex.meshupdate(HighlightMat);
    }
}
