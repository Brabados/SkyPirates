using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/*
 * Tests for Map.SetNeighbours.
 *
 * Board layout used in most tests (3x3):
 *     (0,2) (1,2) (2,2)
 *      (0,1) (1,1) (2,1)
 *  (0,0) (1,0) (2,0)
 *
 * Offsets for flat topped hexes:
 *   (+1,0) (0,+1) (-1,+1) (-1,0) (0,-1) (+1,-1)
 * Offsets for pointy topped hexes:
 *   (0,+1) (-1,0) (-1,-1) (0,-1) (+1,0) (+1,+1)
 *
 *  Flat topped orientation:
 *      NW  N  NE
 *        \ | /
 *   W ---  X  --- E
 *        / | \
 *      SW  S  SE
 *
 *  Pointy topped orientation:
 *        N
 *      W | E
 *     SW X NE
 *        |
 *        S
 */

public class SetNeighboursTests
{
    // Keep track of objects created during a test so they can be cleaned up
    private readonly List<Object> _toDestroy = new List<Object>();

    [TearDown]
    public void Cleanup()
    {
        foreach (var obj in _toDestroy)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
        _toDestroy.Clear();
    }

    // Build a square board entirely in memory. When omit is provided the
    // tile at that coordinate is skipped to test boards with holes.
    private Board CreateBoard(int size, bool isFlatTopped, Vector2Int? omit = null)
    {
        Board board = new Board(new Vector2Int(size, size));
        Map map = new GameObject("TestMap").AddComponent<Map>();
        _toDestroy.Add(map.gameObject);
        map.isFlatTopped = isFlatTopped;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (omit.HasValue && omit.Value.x == x && omit.Value.y == y)
                    continue;
                GameObject go = new GameObject($"Tile_{x}_{y}");
                _toDestroy.Add(go);
                Tile tile = go.AddComponent<Tile>();
                tile.Data = ScriptableObject.CreateInstance<TileDataSO>();
                _toDestroy.Add(tile.Data);
                tile.SetPositionAndHeight(new Vector2Int(x, y), x - size / 2, y - size / 2, 0);
                board.set_Tile(x, y, tile);
            }
        }

        map.SetNeighbours(board, map.isFlatTopped);
        return board;
    }

    private void AssertNeighbours(Board board, bool isFlatTopped)
    {
        var offsets = isFlatTopped ? Map.NeighborOffsetsFlatTopped : Map.NeighborOffsetsPointyTopped;

        foreach (Tile tile in board.GetAllTiles())
        {
            Vector2Int pos = new Vector2Int(tile.Column, tile.Row);
            List<Tile> expected = new List<Tile>();

            foreach (var offset in offsets)
            {
                Vector2Int neighbourPos = pos + offset;
                Tile neighbour = board.get_Tile(neighbourPos.x, neighbourPos.y);
                if (neighbour != null)
                    expected.Add(neighbour);
            }

            CollectionAssert.AreEquivalent(expected, tile.Neighbours, $"Tile at {pos} has incorrect neighbours");
            // verify neighbour symmetry
            foreach (Tile neighbour in tile.Neighbours)
            {
                Assert.Contains(tile, neighbour.Neighbours, $"{tile} missing from {neighbour}'s list");
            }
        }
    }

    [Test]
    public void FlatTopped_SetNeighbours_AssignsCorrectTiles()
    {
        var board = CreateBoard(3, true);
        AssertNeighbours(board, true);
    }

    [Test]
    public void PointyTopped_SetNeighbours_AssignsCorrectTiles()
    {
        var board = CreateBoard(3, false);
        AssertNeighbours(board, false);
    }

    [Test]
    public void SingleTileBoard_HasNoNeighbours()
    {
        var board = CreateBoard(1, true);
        Tile only = board.get_Tile(0, 0);
        Assert.IsNotNull(only);
        Assert.AreEqual(0, only.Neighbours.Count);
    }

    [Test]
    public void MissingTile_IsIgnored()
    {
        // omit the centre tile
        var board = CreateBoard(3, true, new Vector2Int(1, 1));
        AssertNeighbours(board, true);
        foreach (Tile t in board.GetAllTiles())
            Assert.IsFalse(t.Column == 1 && t.Row == 1);
    }

    [Test]
    public void SetNeighbours_CanBeCalledTwice_WithoutDuplication()
    {
        var board = CreateBoard(3, true);

        // capture neighbour counts after first call
        Dictionary<Tile, int> counts = new Dictionary<Tile, int>();
        foreach (Tile t in board.GetAllTiles())
            counts[t] = t.Neighbours.Count;

        // run SetNeighbours again using a new Map instance
        Map extra = new GameObject("ExtraMap").AddComponent<Map>();
        _toDestroy.Add(extra.gameObject);
        extra.isFlatTopped = true;
        extra.SetNeighbours(board, true);

        // ensure counts are unchanged
        foreach (Tile t in board.GetAllTiles())
            Assert.AreEqual(counts[t], t.Neighbours.Count);
    }
}
