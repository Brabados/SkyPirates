using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager EventInstance { get; private set; }
    public BasicControls inputActions { get; private set; }

    // --- EVENTS ---
    public static event Action<GameObject> OnTileHover;
    public static event Action OnTileSelect;
    public static event Action OnTileDeselect;
    public static event Action<Pawn> OnPawnSelect;
    public static event Action<List<Vector3Int>> OnMovementChange;
    public static event Action<Pawn> OnCharacterChange;
    public static event Action<Item> OnItemSelect;
    public static event Action<ItemType, Item> OnEquipmentChange;
    public static event Action<Item> OnInfoChange;
    public static event Action<Item, int[]> OnInfoCompareChange;
    public static event Action OnInfoReset;
    public static event Action<Item> OnInfoCompare;
    public static event Action<Item> OnShowInfo;

    // --- TRIGGERS ---
    public static void TriggerTileHover(GameObject selected) => OnTileHover?.Invoke(selected);
    public static void TriggerTileSelect() => OnTileSelect?.Invoke();
    public static void TriggerTileDeselect() => OnTileDeselect?.Invoke();
    public static void TriggerPawnSelect(Pawn selectedPawn) => OnPawnSelect?.Invoke(selectedPawn);
    public static void TriggerMovementChange(List<Vector3Int> points) => OnMovementChange?.Invoke(points);
    public static void TriggerCharacterChange(Pawn onscreenPawn) => OnCharacterChange?.Invoke(onscreenPawn);
    public static void TriggerItemSelect(Item selectedItem) => OnItemSelect?.Invoke(selectedItem);
    public static void TriggerEquipmentChange(ItemType itemType, Item item) => OnEquipmentChange?.Invoke(itemType, item);
    public static void TriggerInfoChange(Item infoItem) => OnInfoChange?.Invoke(infoItem);
    public static void TriggerInfoCompareChange(Item infoItem, int[] comparisonArray) => OnInfoCompareChange?.Invoke(infoItem, comparisonArray);
    public static void TriggerInfoReset() => OnInfoReset?.Invoke();
    public static void TriggerInfoCompare(Item equippedItem) => OnInfoCompare?.Invoke(equippedItem);
    public static void TriggerShowInfo(Item itemInfo) => OnShowInfo?.Invoke(itemInfo);

    private void Awake()
    {
        if (EventInstance != null && EventInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        EventInstance = this;

        inputActions = new BasicControls();
        inputActions.Battle.Enable();
    }
}
