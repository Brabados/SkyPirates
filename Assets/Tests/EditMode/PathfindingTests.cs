using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PathfindingTests
{
    // Helper that builds a small square board and links neighbour references.
    // Movement costs can be overridden by passing a 2D array of values.
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

    public void FindPath_ReturnsExpectedCoordinates()
    {
        // Arrange: build a 3x3 board with a couple of impassable tiles
        int[,] costs = new int[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                costs[x, y] = 1;

        // Block tiles to force a single route
        costs[0, 1] = 0; // left middle
        costs[1, 0] = 0; // bottom middle

        Board board = CreateBoard(3, costs);
        Tile start = board.get_Tile(0, 0);  // cube (-1,-1,2)
        Tile end = board.get_Tile(2, 1);    // cube (1,0,-1)

        Pathfinding pathfinder = new Pathfinding();
        Tile[] allTiles = board.GetAllTiles().ToArray();

        // Act
        List<Vector3Int> path = pathfinder.FindPath(start, end, allTiles);

        /* Expected route:
         * 1. start (-1,-1,2)
         * 2. centre (0,0,0)
         * 3. end   (1,0,-1)
         * The blocked tiles at (0,1) and (1,0) leave only this path.
         */
        var expected = new List<Vector3Int>
        {
            new Vector3Int(-1, -1, 2),
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, -1)
        };

        // Assert
        Assert.AreEqual(expected, path);

    }
}
