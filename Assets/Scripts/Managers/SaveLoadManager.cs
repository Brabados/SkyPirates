using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;

[System.Serializable]
public class SerializableTile
{
    public string TileTypeID;
    public int Height;
}

[System.Serializable]
public class SerializableBoard
{
    public int x_Height;
    public int y_Width;
    public List<List<SerializableTile>> Tiles;
}

[System.Serializable]
public class ExportData
{
    public List<string> TileTypeIDs;
    public SerializableBoard Board;
}

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager SaveLoadInstance { get; private set; }
    private DirectoryInfo dir;
    public FileInfo[] info;


    public void refreshDirectory()
    {
        info = dir.GetFiles("*.*");
    }
    public void SaveMapToJson(Map map, string filePath)
    {
        var export = new ExportData();
        export.TileTypeIDs = new List<string>();
        Dictionary<TileDataSO, string> tileTypeLookup = new Dictionary<TileDataSO, string>();

        foreach (var tileType in map.TileTypes)
        {
            tileTypeLookup[tileType] = tileType.UniqueID;
            export.TileTypeIDs.Add(tileType.UniqueID);
        }

        export.Board = new SerializableBoard
        {
            x_Height = map.MapSize.x,
            y_Width = map.MapSize.y,
            Tiles = new List<List<SerializableTile>>()
        };

        for (int y = 0; y < map.MapSize.y; y++)
        {
            var row = new List<SerializableTile>();
            for (int x = 0; x < map.MapSize.x; x++)
            {
                Tile tile = map.PlayArea.get_Tile(x, y);
                row.Add(new SerializableTile
                {
                    TileTypeID = tileTypeLookup[tile.Data],
                    Height = (int)tile.Height
                });
            }
            export.Board.Tiles.Add(row);
        }

        string json = JsonConvert.SerializeObject(export, Formatting.Indented);
        File.WriteAllText(filePath, json);
        Debug.Log($"Map saved to: {filePath}");
        refreshDirectory();
    }

    public static Board LoadBoardFromJson(string filePath, Map mapContext, Transform parent)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }

        string json = File.ReadAllText(filePath);
        ExportData data = JsonConvert.DeserializeObject<ExportData>(json);

        Dictionary<string, TileDataSO> idLookup = new Dictionary<string, TileDataSO>();
        foreach (var id in data.TileTypeIDs)
        {
            TileDataSO match = mapContext.TileTypes.Find(t => t.UniqueID == id);
            if (match != null)
            {
                idLookup[id] = match;
            }
            else
            {
                Debug.LogWarning($"Missing TileDataSO with ID: {id}");
            }
        }

        Board board = new Board(new Vector2Int(data.Board.x_Height, data.Board.y_Width));

        for (int y = 0; y < data.Board.y_Width; y++)
        {
            for (int x = 0; x < data.Board.x_Height; x++)
            {
                SerializableTile sTile = data.Board.Tiles[y][x];
                TileDataSO tileType = idLookup.ContainsKey(sTile.TileTypeID) ? idLookup[sTile.TileTypeID] : mapContext.TileTypes[0];

                GameObject holder = new GameObject($"Hex {x},{y}", typeof(Tile));
                Tile tile = holder.GetComponent<Tile>();
                tile.Data = tileType;
                Vector2Int offsetCoords = new Vector2Int(x - data.Board.x_Height / 2, y - data.Board.y_Width / 2);
                Vector3Int cubeCoords = HexUtils.OffsetToCube(offsetCoords, mapContext.isFlatTopped);
                tile.SetPositionAndHeight(offsetCoords, cubeCoords, sTile.Height);
                Vector3 tilePosition = mapContext.GetHexPositionFromCoordinate(new Vector2Int(x, y));
                tilePosition.y += tile.Height / 2;
                holder.transform.position = tilePosition;
                holder.transform.SetParent(parent);
                Instantiate(tile.Data.TilePrefab, holder.transform).transform.position += new Vector3(0, tile.Height / 2 - 1, 0);
                tile.SetupHexRenderer(mapContext.innerSize, mapContext.outerSize, mapContext.isFlatTopped);
                tile.SetPosition(new Vector2Int(x, y));
                tile.SetPawnPos();

                board.set_Tile(x, y, tile);
            }
        }

        return board;
    }

    private void Awake()
    {
        if (SaveLoadInstance != null && SaveLoadInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        SaveLoadInstance = this;
        dir = new DirectoryInfo(Application.persistentDataPath);
        refreshDirectory();

    }
}
