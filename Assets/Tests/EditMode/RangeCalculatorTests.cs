using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RangeCalculatorTests
{
    // Helper that builds a small square board and links neighbour references.
    private Board CreateSimpleBoard(int size)
    {
        Board board = new Board(new Vector2Int(size, size));
        Map map = new GameObject("TestMap").AddComponent<Map>();
        map.isFlatTopped = false;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject go = new GameObject($"Tile_{x}_{y}");
                Tile tile = go.AddComponent<Tile>();
                tile.Data = ScriptableObject.CreateInstance<TileDataSO>();
                tile.Data.MovementCost = 1;

                Vector2Int offset = new Vector2Int(x, y);
                Vector3Int cube = HexUtils.OffsetToCube(offset, map.isFlatTopped);

                tile.SetPositionAndHeight(offset, cube, 0);
                board.set_Tile(x, y, tile);
            }
        }

        map.SetNeighbours(board, map.isFlatTopped);
        return board;
    }

    [Test]
    public void HexRing_ReturnsSixTiles_ForRadiusOne()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        List<Tile> ring = RangeCalculator.HexRing(board, center, 1);
        Assert.AreEqual(6, ring.Count);
    }

    [Test]
    public void HexReachable_RespectsMovementCost()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        Tile neighbour = center.Neighbours[0];
        neighbour.Data.MovementCost = 5;
        List<Tile> reachable = RangeCalculator.HexReachable(board, center, 1);
        Assert.IsFalse(reachable.Contains(neighbour));
    }
}
