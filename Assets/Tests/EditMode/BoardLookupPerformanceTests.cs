using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/*
 * These edit mode tests exercise the cube coordinate lookup path in Board.
 * A simple board is built entirely in memory so no scene assets are required.
 * We then time lookups using the new dictionary based method and compare them
 * with a naive search that scans every tile.  The dictionary lookup should be
 * significantly faster which demonstrates the advantage of caching tiles by
 * their cube coordinates.
 */
public class BoardLookupPerformanceTests
{
    // Helper that builds a square board filled with default tiles.
    private Board CreateBoard(int size)
    {
        Board board = new Board(new Vector2Int(size, size));
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject go = new GameObject($"Tile_{x}_{y}");
                Tile tile = go.AddComponent<Tile>();
                tile.Data = ScriptableObject.CreateInstance<TileDataSO>();
                tile.SetPositionAndHeight(new Vector2Int(x, y), x - size / 2, y - size / 2, 0);
                board.set_Tile(x, y, tile);
            }
        }
        return board;
    }

    // Slow lookup mimicking the old board search implementation.
    private Tile SlowLookup(Board board, Vector3Int cube)
    {
        for (int x = 0; x < board._size_X; x++)
        {
            for (int y = 0; y < board._size_Y; y++)
            {
                Tile t = board.get_Tile(x, y);
                if (t != null && t.QAxis == cube.x && t.RAxis == cube.y && t.SAxis == cube.z)
                    return t;
            }
        }
        return null;
    }

    [Test]
    // Dictionary based lookup should complete faster than iterating every tile.
    public void DictionaryLookup_IsFasterThanNaiveSearch()
    {
        const int size = 20; // 400 tiles
        const int iterations = 1000;
        Board board = CreateBoard(size);

        List<Vector3Int> cubes = new List<Vector3Int>();
        foreach (var tile in board.GetAllTiles())
            cubes.Add(new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis));

        // Use the same set of cube coordinates for both passes
        Stopwatch sw = new Stopwatch();

        sw.Start();
        for (int i = 0; i < iterations; i++)
        {
            foreach (var c in cubes)
                Assert.NotNull(board.GetTileByCube(c));
        }
        sw.Stop();
        long dictTicks = sw.ElapsedTicks;

        sw.Reset();
        sw.Start();
        for (int i = 0; i < iterations; i++)
        {
            foreach (var c in cubes)
                Assert.NotNull(SlowLookup(board, c));
        }
        sw.Stop();
        long slowTicks = sw.ElapsedTicks;

        UnityEngine.Debug.Log($"Dictionary lookup ticks: {dictTicks}, slow lookup ticks: {slowTicks}");
        Assert.Less(dictTicks, slowTicks);
    }

    [Test]
    // After calling Destroy the cube index should no longer contain entries.
    public void Destroy_ClearsCubeIndex()
    {
        Board board = CreateBoard(3);
        List<Vector3Int> cubes = new List<Vector3Int>();
        foreach (var tile in board.GetAllTiles())
            cubes.Add(new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis));

        board.Destroy();

        foreach (var c in cubes)
            Assert.IsNull(board.GetTileByCube(c));
    }
}
