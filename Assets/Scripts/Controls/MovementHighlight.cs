using System.Collections.Generic;
using UnityEngine;

public class MovementHighlight : MonoBehaviour, IHighlightResponce
{
    private GameObject highlightSelect = null;
    private Tile highlightTile = null;
    public Material HighlightMat;
    public List<Tile> Area = new List<Tile>();
    public List<Vector3Int> CurrentPath;
    private Pathfinding pathfinder;
    public PathfinderSelections PathfinderSelections;


    private void Awake()
    {
        pathfinder = new Pathfinding();
        PathfinderSelections = new PathfinderSelections();
        CurrentPath = new List<Vector3Int>();
    }

    public GameObject ReturnHighlight()
    {
        return highlightSelect;
    }

    public void SetHighlight(GameObject highlight)
    {
        if (highlight != null && highlight != highlightSelect)
        {
            Tile HLTile = highlight.GetComponent<Tile>();
            if (Area.Contains(HLTile))
            {
                highlightSelect = highlight;
                highlightTile = HLTile;
                UpdatePath();
            }
        }
    }

    public void MoveHighlight(Vector2 input)
    {
        Tile check = highlightTile.CheckNeighbours(input);
        if (check != null && Area.Contains(check))
        {
            SetHighlight(check.gameObject);
        }
    }

    private void UpdatePath()
    {
        Tile[] areaArray = Area.ToArray();
        List<Vector3Int> path = new List<Vector3Int>();
        List<Tile> route = ((MoveSelect)HexSelectManager.Instance.Responce).Selections;
        if (PathfinderSelections.NumSelections > 0)
        {
            foreach (List<Vector3Int> a in PathfinderSelections.Paths)
                foreach (Vector3Int b in a)
                {
                    path.Add(b);
                }
        }

        CurrentPath = pathfinder.FindPath(route[route.Count - 1], highlightTile, areaArray);
        path.AddRange(CurrentPath);
        EventManager.TriggerMovementChange(path);
    }

    public PathfinderSelections UpdateSelection()
    {

        PathfinderSelections.AddPath(CurrentPath);
        return PathfinderSelections;
    }

    public void Starthighlight(GameObject highlight)
    {
        highlightSelect = highlight;
        highlightTile = highlight.GetComponent<Tile>();
    }

    public void CleanUp()
    {
        highlightSelect = null;
        highlightTile = null;
        Area.Clear();
        CurrentPath.Clear();
        PathfinderSelections.ClearPaths();
    }
}
