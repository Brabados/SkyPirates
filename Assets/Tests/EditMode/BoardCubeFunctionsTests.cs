using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public class BoardCubeFunctionsTests
{
    // Helper to create a simple square board entirely in memory
    private Board CreateBoard(int size)
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
                tile.SetPositionAndHeight(new Vector2Int(x, y), x - size / 2, y - size / 2, 0);
                board.set_Tile(x, y, tile);
            }
        }

        map.SetNeighbours(board, map.isFlatTopped);
        return board;
    }

    [Test]
    public void SearchTileAndGetTileByCube_ReturnSameInstance()
    {
        Board board = CreateBoard(3);

        foreach (Tile tile in board.GetAllTiles())
        {
            Vector3Int cube = new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis);
            Assert.AreSame(tile, board.SearchTileByCubeCoordinates(cube.x, cube.y, cube.z));
            Assert.AreSame(tile, board.GetTileByCube(cube));
        }
    }

    [Test]
    public void CubeDistance_ComputesExpectedValue()
    {
        Board board = CreateBoard(3);
        Tile center = board.get_Tile(1, 1);
        Tile neighbour = board.get_Tile(2, 1);

        int dist = board.CubeDistance(center, neighbour);
        Assert.AreEqual(1, dist);
    }

    [Test]
    public void CubeLerp_ReturnsMidpointBetweenTiles()
    {
        Board board = CreateBoard(3);
        Tile a = board.get_Tile(1, 1); // (0,0,0)
        Tile b = board.get_Tile(2, 1); // (1,0,-1)

        Vector3 expected = new Vector3(0.5f, 0f, -0.5f);
        Vector3 actual = board.CubeLerp(a, b, 0.5f);
        Assert.That(actual, Is.EqualTo(expected).Using(Vector3ComparerWithEqualsOperator.Instance));
    }

    [Test]
    public void CubeRound_RoundsToNearestCube()
    {
        Board board = CreateBoard(1); // board not used but provides method
        Vector3 input = new Vector3(0.6f, -1.2f, 0.6f);

        Vector3Int result = board.CubeRound(input);
        Assert.AreEqual(new Vector3Int(1, -1, 0), result);
    }

    [Test]
    public void GetDirectionVector_ReturnsCorrectDirection()
    {
        Board board = CreateBoard(3);
        Tile from = board.get_Tile(1, 1); // center (0,0,0)
        Tile to = board.get_Tile(2, 1);   // (1,0,-1)

        Vector3Int direction = board.GetDirectionVector(from, to);
        Assert.AreEqual(new Vector3Int(1, 0, -1), direction);
    }
}
