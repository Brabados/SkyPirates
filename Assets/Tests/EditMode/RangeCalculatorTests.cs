using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/*
 * These edit mode tests demonstrate how the pure methods in RangeCalculator
 * can be validated without entering play mode. To run them open the
 * "Test Runner" window in the Unity editor (Window > General > Test Runner),
 * select the **EditMode** tab and click **Run All**.  Each test creates a very
 * small board entirely in memory so nothing in the main scenes is required.
 *
 *  Board layout used in the tests (3x3):
 *      (0,2) (1,2) (2,2)
 *       (0,1) (1,1) (2,1)
 *    (0,0) (1,0) (2,0)
 *  The centre tile at (1,1) is surrounded by six neighbours.  Making the
 *  calculation functions pure means AI coding agents (or humans!) can easily
 *  generate tests like these to confirm behaviour after refactoring.
 */

public class RangeCalculatorTests
{
    // Helper that builds a small square board and links neighbour references.
    // Keeping this code here means each test is entirely self contained.
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
                tile.SetPositionAndHeight(new Vector2Int(x, y), x - size / 2, y - size / 2, 0);
                board.set_Tile(x, y, tile);
            }
        }

        map.SetNeighbours(board, map.isFlatTopped);
        return board;
    }

    [Test]
    // HexRing should return exactly six neighbours around the centre tile
    // when radius is one.
    public void HexRing_ReturnsSixTiles_ForRadiusOne()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        List<Tile> ring = RangeCalculator.HexRing(board, center, 1);
        Assert.AreEqual(6, ring.Count);
    }

    [Test]
    // HexReachable stops when a neighbour would cost more movement than allowed.
    public void HexReachable_RespectsMovementCost()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        // Make one neighbour expensive so it should be ignored
        Tile neighbour = center.Neighbours[0];
        neighbour.Data.MovementCost = 5;
        List<Tile> reachable = RangeCalculator.HexReachable(board, center, 1);

        // The high-cost tile should not be returned because it exceeds the
        // allowed movement budget. This ensures the algorithm respects tile
        // movement cost and highlights why extracting the logic into pure
        // methods makes such checks trivial in tests.
        Assert.IsFalse(reachable.Contains(neighbour));
    }
}
