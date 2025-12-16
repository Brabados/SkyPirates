using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a scrollable list of tile type buttons from the Map's available tiles
/// </summary>
public class TileTypeGenerator : ScrollListGenerator<TileDataSO>
{
    private List<TileDataSO> tileData;

    void Start()
    {
        autoSelectFirstButton = true;
        InitializeTileData();
        RefreshList();
    }

    private void InitializeTileData()
    {
        tileData = new List<TileDataSO>();
        Map playArea = FindObjectOfType<Map>();

        if (playArea != null && playArea.TileTypes != null)
        {
            tileData.AddRange(playArea.TileTypes);
        }
    }

    protected override List<TileDataSO> GetListData() => tileData;

    protected override string GetItemDisplayText(TileDataSO item) => item.Name;

    protected override void ConfigureButton(UnityEngine.UI.Button button, TileDataSO item)
    {
        TileTypeButton tileButton = button.gameObject.AddComponent<TileTypeButton>();
        tileButton.tile = item;
        button.onClick.AddListener(tileButton.SetChange);
    }
}
