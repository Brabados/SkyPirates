using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(RangeFinder))]
public class HexSelectManager : MonoBehaviour
{
    public static HexSelectManager Instance { get; private set; }
    public BasicControls InputActions { get; private set; }

    private ISelectionResponce _responce;

    // Tracks ALL currently selected tiles across ALL selection states.
    public HashSet<Tile> SelectedTiles { get; private set; } = new HashSet<Tile>();

    public Tile LastPawnTile { get; private set; }

    public void UpdateLastPawnTile(Tile tile)
    {
        if (tile != null && tile.Contents != null)
        {
            LastPawnTile = tile;
        }
    }


    // State tracking
    private HexSelectState currentState;
    private readonly HexSelectState defaultState = new DefaultSelectState();
    private readonly HexSelectState moveSelectState = new MoveSelectState();
    private readonly HexSelectState actionSelectState = new ActionSelectState();
    private readonly HexSelectState editSelectState = new EditState();
    private readonly HexSelectState abilitySelectState = new AbilitySelectState();
    private readonly HexSelectState enemyTurnState = new EnemyTurnState();
    // History stack to return to previous state
    private Stack<HexSelectState> stateStack = new Stack<HexSelectState>();

    public ISelectionResponce Responce
    {
        get => _responce;
        set => _responce = value;
    }

    public IHighlightResponce Highlight { get; set; }
    public RangeFinder HighlightFinder { get; private set; }

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

    }

    private void Start()
    {
        EventManager.OnTileSelect += Select;
        EventManager.OnTileDeselect += Deselect;
        EventManager.OnTileHover += SetHighlight;

        InputActions = EventManager.EventInstance.inputActions;
        HighlightFinder = GetComponent<RangeFinder>();

        string name = SceneManager.GetActiveScene().name;
        if (name == "BattleScene")
        {
            currentState = defaultState;
            currentState.EnterState(this);
        }
        else if (name == "ShipBuildScreen")
        {
            currentState = editSelectState;
            currentState.EnterState(this);
        }
    }

    private void Update()
    {
        currentState.UpdateState(this);
    }

    private void OnDestroy()
    {
        EventManager.OnTileSelect -= Select;
        EventManager.OnTileDeselect -= Deselect;
        EventManager.OnTileHover -= SetHighlight;
    }

    public void Select()
    {
        Responce?.Select(Highlight?.ReturnHighlight());
    }

    public void Deselect()
    {
        Responce?.Deselect();
    }

    public void SetHighlight(GameObject toHighlight)
    {
        Highlight?.SetHighlight(toHighlight);
    }

    // These now push the current state to the stack to enable state rollback
    public void SwitchToMoveSelectState()
    {
        stateStack.Push(currentState);
        currentState.ExitState(this);
        currentState = moveSelectState;
        currentState.EnterState(this);
        Debug.Log("MoveState");
    }

    public void SwitchToDefaultState()
    {
        stateStack.Push(currentState);
        currentState.ExitState(this);
        currentState = defaultState;
        currentState.EnterState(this);
        EventManager.TriggerActionExicuted(true);
        EventManager.TriggerMovementAllUsed(true);
        Debug.Log("DefaultState");
    }

    public void SwitchToEnemyTurnState()
    {
        stateStack.Push(currentState);
        currentState.ExitState(this);
        currentState = enemyTurnState;
        currentState.EnterState(this);
    }


    public void SwitchToActionSelectState()
    {
        stateStack.Push(currentState);
        currentState.ExitState(this);
        currentState = actionSelectState;
        currentState.EnterState(this);
        Debug.Log("ActionState");
    }

    // Return to the previous state
    public void ReturnToPreviousState()
    {
        if (stateStack.Count > 0)
        {
            currentState.ExitState(this);
            currentState = stateStack.Pop();
            currentState.EnterState(this);
            Debug.Log("Returned to previous state");
        }
        else
        {
            Debug.LogWarning("No previous state to return to.");
        }
    }

    public void SwitchToAbilityState(ActiveAbility Ability)
    {
        stateStack.Push(currentState);
        currentState.ExitState(this);
        ((AbilitySelectState)abilitySelectState).Active = Ability;
        currentState = abilitySelectState;
        currentState.EnterState(this);
        Debug.Log("Ability State" + " " + Ability.Name);
    }

    public void UpdateMovementRange(List<Tile> area, Tile selection)
    {
        if (((MoveSelect)Responce).Selections.Count > 0)
        {
            Tile lastSelectedTile = selection;
            List<Tile> movementRange = area;
            PathfinderSelections paths = ((MovementHighlight)Highlight).UpdateSelection();
            int remainingMovement = TurnManager.Instance.currentTurn?.MovementLeft
                         ?? ((MoveSelect)Responce).SelectedCharater.Stats.Movement;

            foreach (List<Vector3Int> path in paths.Paths)
            {
                remainingMovement -= path.Count;
            }
            remainingMovement += paths.Paths.Count;

            foreach (Tile tile in movementRange)
            {
                tile.Hex.meshupdate(tile.BaseMaterial);
            }

            movementRange = HighlightFinder.HexReachable(lastSelectedTile, remainingMovement);
            ((MoveSelect)Responce).SetPaths(paths);
            ((MoveSelect)Responce).Area = movementRange;
            ((MovementHighlight)Highlight).Area = movementRange;

            foreach (Tile tile in movementRange)
            {
                tile.Hex.meshupdate(((MoveSelect)Responce).HighlightMat);
            }
        }
    }

    public Tile GetCurrentSelectedTile()
    {
        foreach (var tile in SelectedTiles)
        {
            return tile;
        }
        return null;
    }

}
