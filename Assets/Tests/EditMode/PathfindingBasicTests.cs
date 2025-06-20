using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Basic checks for Pathfinding.FindPath on a small board.
/// </summary>
public class PathfindingBasicTests
{
    // Build a square board entirely in memory. Movement costs can be provided
    // to block tiles if needed.
    private Board CreateBoard(int size, int[,] movementCosts = null)
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
                tile.Data.MovementCost = movementCosts == null ? 1 : movementCosts[x, y];
                tile.SetPositionAndHeight(new Vector2Int(x, y), x - size / 2, y - size / 2, 0);
                board.set_Tile(x, y, tile);
            }
        }

        map.SetNeighbours(board, map.isFlatTopped);
        return board;
    }

    [Test]
    public void FindPath_StartToEnd_ReturnsCorrectLengthAndEndpoints()
    {
        Board board = CreateBoard(3);
        Tile start = board.get_Tile(0, 0); // cube (-1,-1,2)
        Tile end = board.get_Tile(2, 2);   // cube (1,1,-2)

        Pathfinding finder = new Pathfinding();
        Tile[] allTiles = board.GetAllTiles().ToArray();
        List<Vector3Int> path = finder.FindPath(start, end, allTiles);

        // Path should include both start and end tiles.
        Assert.AreEqual(start.ReturnSquareCoOrds(), path.First());
        Assert.AreEqual(end.ReturnSquareCoOrds(), path.Last());

        // Length should equal distance + 1 to include the starting tile.
        int expectedLength = board.CubeDistance(start, end) + 1;
        Assert.AreEqual(expectedLength, path.Count);
    }

    [Test]
    public void FindPath_NoRoute_ReturnsEmptyList()
    {
        int size = 3;
        int[,] costs = new int[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                costs[x, y] = 1;

        // Impassable wall separating start and end
        for (int y = 0; y < size; y++)
            costs[1, y] = 0;

        Board board = CreateBoard(size, costs);
        Tile start = board.get_Tile(0, 1);
        Tile end = board.get_Tile(2, 1);

        Pathfinding finder = new Pathfinding();
        Tile[] allTiles = board.GetAllTiles().ToArray();
        List<Vector3Int> path = finder.FindPath(start, end, allTiles);

        Assert.AreEqual(0, path.Count);
    }
}
