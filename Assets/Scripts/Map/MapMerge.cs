using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShipSide
{
    Bow,       
    Stern,      
    Port,       
    Starboard   
}

public class MapMerge : MonoBehaviour
{
    // Direction vectors for pointy-topped layout
    private static readonly Dictionary<ShipSide, Vector2Int> PointyOffsets = new Dictionary<ShipSide, Vector2Int>
    {
        { ShipSide.Bow, new Vector2Int(0, -1) },   // Up
        { ShipSide.Stern, new Vector2Int(0, 1) },    // Down
        { ShipSide.Port, new Vector2Int(-1, 0) },   // Left
        { ShipSide.Starboard, new Vector2Int(1, 0) }     // Right
    };

    // Direction vectors for flat-topped layout
    private static readonly Dictionary<ShipSide, Vector2Int> FlatOffsets = new Dictionary<ShipSide, Vector2Int>
    {
        { ShipSide.Bow, new Vector2Int(1, 0) },    // Right
        { ShipSide.Stern, new Vector2Int(-1, 0) },   // Left
        { ShipSide.Port, new Vector2Int(0, -1) },   // Up
        { ShipSide.Starboard, new Vector2Int(0, 1) }     // Down
    };

    /// <summary>
    /// Calculate offsets for two boards when merged together. This function is
    /// pure so the math can be unit tested without any scene objects.
    /// </summary>
    public static (Vector2Int offsetA, Vector2Int offsetB, Vector2Int merged)
        GetMergeLayout(Vector2Int sizeA, Vector2Int sizeB, ShipSide side)
    {
        Vector2Int offsetA = Vector2Int.zero;
        Vector2Int offsetB = Vector2Int.zero;
        Vector2Int merged = Vector2Int.zero;

        switch (side)
        {
            case ShipSide.Bow:
                // Ship B sits above ship A
                offsetB = new Vector2Int(0, sizeA.y);
                merged = new Vector2Int(Mathf.Max(sizeA.x, sizeB.x), sizeA.y + sizeB.y);
                break;
            case ShipSide.Stern:
                // Ship B attaches below, so shift ship A up
                offsetA = new Vector2Int(0, sizeB.y);
                merged = new Vector2Int(Mathf.Max(sizeA.x, sizeB.x), sizeA.y + sizeB.y);
                break;
            case ShipSide.Port:
                // Ship B attaches on the left
                offsetA = new Vector2Int(sizeB.x, 0);
                merged = new Vector2Int(sizeA.x + sizeB.x, Mathf.Max(sizeA.y, sizeB.y));
                break;
            case ShipSide.Starboard:
                // Ship B attaches on the right
                offsetB = new Vector2Int(sizeA.x, 0);
                merged = new Vector2Int(sizeA.x + sizeB.x, Mathf.Max(sizeA.y, sizeB.y));
                break;
        }

        return (offsetA, offsetB, merged);
    }

    public static void MergeBoards(Map map, Board shipA, Board shipB, ShipSide sideToAttach)
    {
        bool pointyTopped = !map.isFlatTopped;
        var offsets = pointyTopped ? PointyOffsets : FlatOffsets;
        Vector2Int dir = offsets[sideToAttach];

        int widthA = shipA._size_X;
        int heightA = shipA._size_Y;
        int widthB = shipB._size_X;
        int heightB = shipB._size_Y;

        // Determine where each board should be positioned in the merged grid.
        var layout = GetMergeLayout(new Vector2Int(widthA, heightA), new Vector2Int(widthB, heightB), sideToAttach);

        int offsetXA = layout.offsetA.x;
        int offsetYA = layout.offsetA.y;
        int offsetXB = layout.offsetB.x;
        int offsetYB = layout.offsetB.y;
        int mergedWidth = layout.merged.x;
        int mergedHeight = layout.merged.y;

        map.PlayArea = new Board(new Vector2Int(mergedWidth, mergedHeight));

        // Copy Ship A tiles with offset
        for (int x = 0; x < widthA; x++)
        {
            for (int y = 0; y < heightA; y++)
            {
                Tile tile = shipA.get_Tile(x, y);
                if (tile != null)
                {
                    int mx = x + offsetXA;
                    int my = y + offsetYA;
                    Vector2Int tilePos = new Vector2Int(mx, my);
                    Vector2Int centeredOffset = tilePos - new Vector2Int(mergedWidth / 2, mergedHeight / 2);
                    Vector3Int cubeCoords = HexUtils.OffsetToCube(centeredOffset, map.isFlatTopped);


                    Tile newTile = Object.Instantiate(tile, map.transform);
                    newTile.SetPosition(tilePos);
                    newTile.SetQUSPosition(cubeCoords);
                    newTile.SetPawnPos();

                    map.PlayArea.set_Tile(mx, my, newTile);
                    newTile.transform.position = map.GetHexPositionFromCoordinate(tilePos);
                }
            }
        }

        // Copy Ship B tiles with offset
        for (int x = 0; x < widthB; x++)
        {
            for (int y = 0; y < heightB; y++)
            {
                Tile tile = shipB.get_Tile(x, y);
                if (tile != null)
                {
                    int mx = x + offsetXB;
                    int my = y + offsetYB;
                    Vector2Int tilePos = new Vector2Int(mx, my);
                    Vector2Int centeredOffset = tilePos - new Vector2Int(mergedWidth / 2, mergedHeight / 2);
                    Vector3Int cubeCoords = HexUtils.OffsetToCube(centeredOffset, map.isFlatTopped);


                    Tile newTile = Object.Instantiate(tile, map.transform);
                    newTile.SetPosition(tilePos);
                    newTile.SetQUSPosition(cubeCoords);
                    newTile.SetPawnPos();

                    map.PlayArea.set_Tile(mx, my, newTile);
                    newTile.transform.position = map.GetHexPositionFromCoordinate(tilePos);
                }
            }
        }


        map.MapSize = new Vector2Int(mergedWidth, mergedHeight);

        FillNulls(map.PlayArea, map);
        // Set neighbors and first hex
        map.SetNeighbours(map.PlayArea, map.isFlatTopped);
        map.setFirstHex();

        Debug.Log("Merged boards into map.");
    }

    public static void FillNulls(Board board, Map mapData)
    {
        int sizeX = board._size_X;
        int sizeY = board._size_Y;

        int qStart = -sizeX / 2;
        int rStart = -sizeY / 2;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Tile existing = board.get_Tile(x, y);
                if (existing == null)
                {
                    int q = qStart + x;
                    int r = rStart + y;

                    // Create empty GameObject with Tile component
                    GameObject holder = new GameObject($"Hex {x},{y}", typeof(Tile));
                    Tile tile = holder.GetComponent<Tile>();
                    tile.Data = mapData.TileTypes[0];

                    // Setup position, height, and cube coords
                    Vector2Int offset = new Vector2Int(x, y);
                    Vector3Int cube = HexUtils.OffsetToCube(offset, mapData.isFlatTopped);
                    tile.SetPositionAndHeight(offset, cube, tile.Data == mapData.TileTypes[0] ? 5 : 20);

                    // Position the tile in world space
                    Vector3 worldPos = mapData.GetHexPositionFromCoordinate(new Vector2Int(x, y));
                    worldPos.y += tile.Height / 2f;
                    holder.transform.position = worldPos;

                    // Set parent to map object for hierarchy cleanliness
                    holder.transform.SetParent(mapData.transform);

                    // Instantiate visual mesh prefab under this tile
                    GameObject visual = Object.Instantiate(tile.Data.TilePrefab, holder.transform);
                    visual.transform.position += new Vector3(0, tile.Height / 2f - 1f, 0);

                    // Setup mesh, material, size, etc.
                    tile.SetupHexRenderer(mapData.innerSize, mapData.outerSize, mapData.isFlatTopped);

                    // Set again in case something changed
                    tile.SetPosition(new Vector2Int(x, y));
                    tile.SetPawnPos();

                    // Assign to board
                    board.set_Tile(x, y, tile);
                }
            }
        }
    }

}

