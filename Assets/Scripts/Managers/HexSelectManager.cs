using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Manager for the selection and highlight of hexes. Informs the current IHighlightResponce and ISelectionResponce when to
//act with given inputs.
[RequireComponent(typeof(RangeFinder))]
public class HexSelectManager : MonoBehaviour
{
    public static HexSelectManager Instance { get; private set; }
    public BasicControls InputActions { get; private set; }
    public ISelectionResponce Responce { get; set; }
    public IHighlightResponce Highlight { get; set; }
    public RangeFinder HighlightFinder { get; private set; }

    public Canvas UI { get; private set; }

    private HexSelectState currentState;
    private readonly HexSelectState defaultState = new DefaultSelectState();
    private readonly HexSelectState moveSelectState = new MoveSelectState();
    private readonly HexSelectState actionSelectState = new ActionSelectState();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        currentState = defaultState;
        currentState.EnterState(this);
    }

    private void Start()
    {
        EventManager.OnTileSelect += Select;
        EventManager.OnTileDeselect += Deselect;
        EventManager.OnTileHover += SetHighlight;
        InputActions = EventManager.EventInstance.inputActions;
        HighlightFinder = GetComponent<RangeFinder>();
        UI = FindObjectOfType<Canvas>();
        UI.enabled = false;
    }

    private void Update()
    {
        currentState.UpdateState(this);
    }

    public void Select()
    {
        Responce.Select(Highlight.ReturnHighlight());
    }

    public void Deselect()
    {
        Responce.Deselect();
    }

    public void SetHighlight(GameObject toHighlight)
    {
        Highlight.SetHighlight(toHighlight);
    }

    public void SwitchToMoveSelectState()
    {
        currentState.ExitState(this);
        currentState = moveSelectState;
        currentState.EnterState(this);
        Debug.Log("MoveState");
    }

    public void SwitchToDefaultState()
    {
        currentState.ExitState(this);
        currentState = defaultState;
        currentState.EnterState(this);
        Debug.Log("DefultState");
    }

    public void SwitchToActionSelectState()
    {
        currentState.ExitState(this);
        currentState = actionSelectState;
        currentState.EnterState(this);
        Debug.Log("ActionState");
    }

    private void OnDestroy()
    {
        EventManager.OnTileSelect -= Select;
        EventManager.OnTileDeselect -= Deselect;
        EventManager.OnTileHover -= SetHighlight;
    }

    public void UpdateMovementRange(List<Tile> Area, Tile Selection)
    {
        if (((MoveSelect)Responce).Selections.Count > 0)
        {
            Tile lastSelectedTile = Selection;
            List<Tile> movementRange = Area;
            PathfinderSelections Paths = ((MovementHighlight)Highlight).UpdateSelection();
            int remainingMovement = ((MoveSelect)Responce).SelectedCharater.Stats.Movement;
            foreach (List<Vector3Int> a in Paths.Paths)
            {
                remainingMovement -= a.Count;
            }
            remainingMovement += Paths.Paths.Count;
            foreach (Tile tile in movementRange)
            {
                tile.Hex.meshupdate(tile.BaseMaterial);
            }
            movementRange = HexSelectManager.Instance.HighlightFinder.HexReachable(lastSelectedTile, remainingMovement);
            ((MoveSelect)Responce).SetPaths(Paths);
            ((MoveSelect)Responce).Area = movementRange;
            ((MovementHighlight)Highlight).Area = movementRange;

            foreach (Tile tile in movementRange)
            {
                tile.Hex.meshupdate(((MoveSelect)Responce).HighlightMat);
            }
        }
    }
}