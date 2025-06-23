using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSelect : MonoBehaviour, ISelectionResponce
{
    public Material selectedMat;
    public GameObject SelectedObject { get; private set; } = null;
    public Tile SelectedTile { get; private set; } = null;
    public Pawn SelectedContents { get; private set; } = null;

    [UnityEngine.Serialization.FormerlySerializedAs("CharaterNo")]
    public int CharacterNo;

    private bool isReselecting = false;

    public void Start()
    {
        CharacterNo = -1;
    }

    public GameObject CurrentSelection()
    {
        return SelectedObject;
    }

    public void Deselect()
    {
        if (SelectedObject != null)
        {
            // Only remove visual if you are truly deselecting
            if (!isReselecting)
            {
                SelectedTile.Hex.meshupdate(SelectedTile.BaseMaterial);

                // If this is the player intentionally deselecting, remove from the global tracker
                HexSelectManager.Instance.SelectedTiles.Remove(SelectedTile);

                SelectedTile = null;
                SelectedContents = null;
                SelectedObject = null;
            }
        }

        // Only return to default state if this is a true deselection
        if (CharacterNo == -1 && !isReselecting)
        {
            HexSelectManager.Instance.SwitchToDefaultState();
        }
    }

    public void Select(GameObject Selection)
    {
        if (Selection != null && SelectedObject == null)
        {
            // First time selection
            SelectedObject = Selection;
            SelectedTile = SelectedObject.GetComponent<Tile>();
            SelectedContents = SelectedTile.Contents;

            if (SelectedContents != null && SelectedContents is PlayerPawns)
            {
                EventManager.TriggerPawnSelect(SelectedContents);
                EventManager.TriggerUIUpdate(SelectedContents);
            }

            SelectedTile.Hex.meshupdate(selectedMat);
            HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);
        }
        else if (Selection != null)
        {
            // Reselection (no recursion)

            // Remove previous selection visual
            if (SelectedTile != null)
            {
                SelectedTile.Hex.meshupdate(SelectedTile.BaseMaterial);
                HexSelectManager.Instance.SelectedTiles.Remove(SelectedTile);
            }

            // Assign new selection
            SelectedObject = Selection;
            SelectedTile = SelectedObject.GetComponent<Tile>();
            SelectedContents = SelectedTile.Contents;

            if (SelectedContents != null && SelectedContents is PlayerPawns)
            {
                EventManager.TriggerPawnSelect(SelectedContents);
                EventManager.TriggerUIUpdate(SelectedContents);
            }

            SelectedTile.Hex.meshupdate(selectedMat);
            HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);
        }
    }

}
