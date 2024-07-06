using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelect : MonoBehaviour, ISelectionResponce
{
    public Material HighlightMat;
    public Pawn SelectedCharater;
    public List<Tile> Area = new List<Tile>();
    public List<Tile> Selections = new List<Tile>();
    public PathfinderSelections PathfinderSelections;
    public RangeFinder range;


    public GameObject SelectedObject { get; private set; } = null;

    private void Awake()
    {
        PathfinderSelections = new PathfinderSelections();

    }

    private void Start()
    {
        range = FindObjectOfType<RangeFinder>();
    }
    public GameObject CurrentSelection()
    {
        return SelectedObject;
    }

    public void Deselect()
    {

        Tile SLTile = null;
        if (SelectedObject != null)
        {
            SLTile = SelectedObject.GetComponent<Tile>();
        }
        else
        {
            HexSelectManager.Instance.SwitchToDefaultState();
            EventManager.MovementChangeTrigger(null);
        }
        if (SLTile != null)
        {
            Selections.Remove(SLTile);
            PathfinderSelections.ClearLastPath();
            ((MovementHighlight)HexSelectManager.Instance.Highlight).PathfinderSelections = PathfinderSelections;
            if (Selections.Count == 0)
            {
                HexSelectManager.Instance.SwitchToDefaultState();
                EventManager.MovementChangeTrigger(null);
            }
            else
            {
                SelectedObject = Selections[Selections.Count - 1].gameObject;
                int remainingMovement = SelectedCharater.Stats.Movement;
                if (Selections.Count != 1)
                {
                    remainingMovement += 1;
                }
                foreach (List<Vector3Int> a in PathfinderSelections.Paths)
                {
                    remainingMovement -= a.Count;
                }
                Area = range.HexReachable(SelectedObject.GetComponent<Tile>(), remainingMovement);
                ((MovementHighlight)HexSelectManager.Instance.Highlight).Area = Area;
                foreach (Tile tile in Area)
                {
                    tile.Hex.meshupdate(((MoveSelect)HexSelectManager.Instance.Responce).HighlightMat);
                }
            }
        }
    }

    public void Select(GameObject selection)
    {
        Tile tile = selection.GetComponent<Tile>();
        if(tile != null && SelectedObject != null)
        {
            if (tile == SelectedObject.GetComponent<Tile>())
            {
                SelectedCharater.Position = tile;
                tile.Contents = SelectedCharater;
                Selections[0].Contents = null;
                //Currently hard coded hight change for test cubes, not 100% sure why the diffrence in hight is 4.5. Investigate and set dynamicly 
                Vector3 Position = new Vector3(tile.PawnPosition.transform.position.x, tile.PawnPosition.transform.position.y + 4.5f, tile.PawnPosition.transform.position.z);
                SelectedCharater.gameObject.transform.position = Position;
                HexSelectManager.Instance.SwitchToDefaultState();
                EventManager.MovementChangeTrigger(null);
                return;
            }
        }
        if (tile != null)
        {
            SelectedObject = selection;
            Selections.Add(tile);
            HexSelectManager.Instance.UpdateMovementRange(Area, Selections[Selections.Count - 1]);
        }
    }

    public void SetPaths(PathfinderSelections ToAdd)
    {
        PathfinderSelections = ToAdd;
    }

    public void CleanUP()
    {
        SelectedCharater = null;
        Area.Clear();
        Selections.Clear();
        PathfinderSelections.ClearPaths();
    }
}
