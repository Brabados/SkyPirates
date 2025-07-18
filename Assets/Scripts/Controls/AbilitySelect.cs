using System.Collections.Generic;
using UnityEngine;

public class AbilitySelect : MonoBehaviour, ISelectionResponce
{
    public Material HighlightMat;
    public ActiveAbility ActiveAbility;
    public List<Tile> Area = new List<Tile>();

    public GameObject SelectedObject { get; private set; } = null;

    public GameObject CurrentSelection()
    {
        return SelectedObject;
    }

    public void Select(GameObject selection)
    {
        Tile tile = selection.GetComponent<Tile>();

        if (tile != null && Area.Contains(tile))
        {
            SelectedObject = selection;

            // Selection complete. Trigger ability resolution here or pass to next system.
            Debug.Log("Ability selected at tile: " + tile.name);
            TurnManager.Instance.currentTurn.ActionTaken = true;
            EventManager.TriggerActionExicuted();

            // Return to previous state after selection
            HexSelectManager.Instance.ReturnToPreviousState();
        }
    }

    public void Deselect()
    {
        HexSelectManager.Instance.ReturnToPreviousState();
    }

    public void CleanUp()
    {
        Area.Clear();
        SelectedObject = null;
    }
}
