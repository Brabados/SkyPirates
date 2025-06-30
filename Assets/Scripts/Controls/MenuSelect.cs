using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Menues
{
    CombatButtons,
    PawnInfo
}

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

    public void Select(GameObject selection)
    {
        if (selection != null && SelectedObject == null)
        {
            SelectedObject = selection;
            SelectedTile = SelectedObject.GetComponent<Tile>();
            SelectedContents = SelectedTile.Contents;


            if (SelectedContents != null && SelectedContents is PlayerPawns)
            {
                EventManager.TriggerShowCanvas(((int)Menues.PawnInfo));
                HexSelectManager.Instance.UpdateLastPawnTile(SelectedTile);
                EventManager.TriggerPawnSelect(SelectedContents);
                EventManager.TriggerUIUpdate(SelectedContents);
            }

            SelectedTile.Hex.meshupdate(selectedMat);
            HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);
        }
        else if (selection != null)
        {
            
            if (SelectedTile != null)
            {
                SelectedTile.Hex.meshupdate(SelectedTile.BaseMaterial);
                HexSelectManager.Instance.SelectedTiles.Remove(SelectedTile);
            }

            SelectedObject = selection;
            SelectedTile = SelectedObject.GetComponent<Tile>();
            SelectedContents = SelectedTile.Contents;

            // Track this as the last pawn tile if it has a pawn


            if (SelectedContents != null && SelectedContents is PlayerPawns)
            {
                EventManager.TriggerShowCanvas(((int)Menues.PawnInfo));
                HexSelectManager.Instance.UpdateLastPawnTile(SelectedTile);
                EventManager.TriggerPawnSelect(SelectedContents);
                EventManager.TriggerUIUpdate(SelectedContents);
            }

            SelectedTile.Hex.meshupdate(selectedMat);
            HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);
        }
    }


    public void SetSelection(GameObject selection)
    {
        if (selection == null) return;

        SelectedObject = selection;
        SelectedTile = selection.GetComponent<Tile>();
        SelectedContents = SelectedTile?.Contents;

        // Optional: Re-highlight selection material if needed
        SelectedTile.Hex.meshupdate(selectedMat);

        HexSelectManager.Instance.SelectedTiles.Add(SelectedTile);
    }


}
