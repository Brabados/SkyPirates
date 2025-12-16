using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles switching between player characters and managing their on-screen visibility
/// </summary>
public class CharacterSelector : MonoBehaviour
{
    [Header("Scene Positions")]
    [SerializeField] private Transform onScreenPosition;
    [SerializeField] private Transform storagePosition;

    private List<PlayerPawns> playerPawns = new List<PlayerPawns>();
    private int currentPawnIndex = 0;
    private BasicControls inputActions;
    private bool hasInitialized = false;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        inputActions = EventManager.EventInstance.inputActions;
        LoadPlayerPawns();
        ShowCurrentPawn();

        EventManager.OnEquipmentChange += OnEquipmentChanged;
        hasInitialized = false;
    }

    private void LoadPlayerPawns()
    {
        playerPawns.Clear();

        if (PawnManager.PawnManagerInstance?.PlayerPawns != null)
        {
            foreach (PlayerPawns pawn in PawnManager.PawnManagerInstance.PlayerPawns)
            {
                MovePawn(pawn, storagePosition.position);
                playerPawns.Add(pawn);
            }
        }
    }

    void Update()
    {
        // Ensure character display updates on first frame
        if (!hasInitialized)
        {
            EventManager.TriggerCharacterChange(GetCurrentPawn());
            hasInitialized = true;
        }

        HandleCharacterSwitch();
    }

    private void HandleCharacterSwitch()
    {
        if (!inputActions.Menu.SwitchCharater.triggered) return;

        HideCurrentPawn();

        float input = inputActions.Menu.SwitchCharater.ReadValue<float>();
        currentPawnIndex = WrapIndex(currentPawnIndex + (int)input, playerPawns.Count);

        ShowCurrentPawn();
    }

    private void ShowCurrentPawn()
    {
        if (playerPawns.Count == 0) return;

        PlayerPawns currentPawn = GetCurrentPawn();
        currentPawn.Equiped.Onscreen = true;
        MovePawn(currentPawn, onScreenPosition.position);
        EventManager.TriggerCharacterChange(currentPawn);
    }

    private void HideCurrentPawn()
    {
        if (playerPawns.Count == 0) return;

        PlayerPawns currentPawn = GetCurrentPawn();
        currentPawn.Equiped.Onscreen = false;
        MovePawn(currentPawn, storagePosition.position);
    }

    private void OnEquipmentChanged(ItemType itemType, Item item)
    {
        PlayerPawns currentPawn = GetCurrentPawn();
        currentPawn.Equiped.UpdateEquipment(itemType, item);

        // Sync with master list
        if (PlayerList.ListInstance?.AllPlayerPawns != null &&
            currentPawnIndex < PlayerList.ListInstance.AllPlayerPawns.Count)
        {
            PlayerList.ListInstance.AllPlayerPawns[currentPawnIndex]
                .GetComponent<PlayerPawns>()?.Equiped.UpdateEquipment(itemType, item);
        }

        EventManager.TriggerCharacterChange(currentPawn);
    }

    private PlayerPawns GetCurrentPawn()
    {
        return playerPawns.Count > 0 ? playerPawns[currentPawnIndex] : null;
    }

    private void MovePawn(PlayerPawns pawn, Vector3 position)
    {
        if (pawn != null)
            pawn.transform.position = position;
    }

    private int WrapIndex(int index, int count)
    {
        if (count == 0) return 0;
        return ((index % count) + count) % count;
    }

    void OnEnable()
    {
        hasInitialized = false;
    }

    void OnDestroy()
    {
        EventManager.OnEquipmentChange -= OnEquipmentChanged;
    }
}
