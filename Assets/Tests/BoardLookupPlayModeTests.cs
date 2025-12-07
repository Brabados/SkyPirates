using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardLookupPlayModeTests
{
    private Map CreateBasicMap(Vector2Int size)
    {
        var go = new GameObject("Map");
        var map = go.AddComponent<Map>();
        map.MapSize = size;
        map.innerSize = 0.5f;
        map.outerSize = 1f;
        map.isFlatTopped = true;
        map.TileTypes = new List<TileDataSO>();

        var tile1 = ScriptableObject.CreateInstance<TileDataSO>();
        tile1.UniqueID = "type1";
        tile1.TilePrefab = new GameObject("prefab1");
        tile1.BaseMat = new Material(Shader.Find("Standard"));
        map.TileTypes.Add(tile1);

        var tile2 = ScriptableObject.CreateInstance<TileDataSO>();
        tile2.UniqueID = "type2";
        tile2.TilePrefab = new GameObject("prefab2");
        tile2.BaseMat = tile1.BaseMat;
        map.TileTypes.Add(tile2);

        return map;
    }

    private void EnsureCamera()
    {
        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.AddComponent<Camera>();
            camGO.AddComponent<CameraController>();
        }
    }

    private void AssertBoardLookups(Board board)
    {
        foreach (var tile in board.GetAllTiles())
        {
            var cube = new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis);
            var found = board.SearchTileByCubeCoordinates(cube.x, cube.y, cube.z);

            Assert.IsNotNull(found, $"Lookup failed for tile with cube=({cube.x},{cube.y},{cube.z})");
            Assert.AreEqual(tile, found, $"Mismatch at cube=({cube.x},{cube.y},{cube.z})");
        }
    }

    private void AssertCubeOriginIsOffset00(Board board)
    {
        var tile = board.get_Tile(0, 0);
        Assert.NotNull(tile, "No tile at offset (0,0)");
        Assert.AreEqual(0, tile.QAxis, "Tile at (0,0) does not have Q=0");
        Assert.AreEqual(0, tile.RAxis, "Tile at (0,0) does not have R=0");
        Assert.AreEqual(0, tile.SAxis, "Tile at (0,0) does not have S=0");

        var found = board.SearchTileByCubeCoordinates(0, 0, 0);
        Assert.AreSame(tile, found, "Lookup for (0,0,0) did not return tile at (0,0)");
    }

    [UnityTest]
    public IEnumerator RandomGenerationBoardLookup()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(3, 3));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();

        Board board = gen.Generate(map);
        yield return null;

        AssertBoardLookups(board);
        AssertCubeOriginIsOffset00(board);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }

    [UnityTest]
    public IEnumerator LoadBoardFromJsonLookup()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(2, 2));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        map.PlayArea = gen.Generate(map);

        var saveGO = new GameObject("SaveLoad");
        var slm = saveGO.AddComponent<SaveLoadManager>();
        yield return null;

        string path = Path.Combine(Application.persistentDataPath, "temp_board.json");
        slm.SaveMapToJson(map, path);

        Board loaded = SaveLoadManager.LoadBoardFromJson(path, map, map.transform);
        yield return null;

        AssertBoardLookups(loaded);
        AssertCubeOriginIsOffset00(loaded);

        Object.Destroy(genGO);
        Object.Destroy(saveGO);
        Object.Destroy(map.gameObject);
    }

    [UnityTest]
    public IEnumerator MergeBoardsLookup()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(2, 2));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        Board a = gen.Generate(map);
        Board b = gen.Generate(map);

        MapMerge.MergeBoards(map, a, b, ShipSide.Bow);
        yield return null;

        AssertBoardLookups(map.PlayArea);
        AssertCubeOriginIsOffset00(map.PlayArea);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }

    [UnityTest]
    public IEnumerator MergeBoards_RespectsMapOrientation([Values(true, false)] bool flat)
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(2, 2));
        map.isFlatTopped = flat;

        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        Board a = gen.Generate(map);
        Board b = gen.Generate(map);

        MapMerge.MergeBoards(map, a, b, ShipSide.Bow);
        yield return null;

        foreach (var tile in map.PlayArea.GetAllTiles())
        {
            Assert.AreEqual(flat, tile.Hex.isFlatTopped, $"Tile at {tile.QAxis},{tile.RAxis} has wrong orientation.");
        }

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }

    [UnityTest]
    public IEnumerator RotateBoardLookup()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(3, 3));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        Board board = gen.Generate(map);

        Board rotated = BoardRotator.RotateBoard(board, BoardRotator.Rotation.Rotate60CW);
        yield return null;

        AssertBoardLookups(rotated);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }
}
