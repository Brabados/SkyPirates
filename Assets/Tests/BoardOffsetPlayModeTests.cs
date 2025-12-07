// PlayMode tests verifying that board generators correctly calculate axial offsets.
// ------------------------------------------------------------------------------
// Run these from Unity's Test Runner (Window > General > Test Runner) under the
// PlayMode tab. Each test builds a minimal scene at runtime.

using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardOffsetPlayModeTests
{
    // Helper used by the play mode tests to create a trivial Map object with two
    // placeholder tile types.
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

    // Many generation utilities expect a MainCamera in the scene. These tests
    // create one if it does not already exist.
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

    // RandomGeneration should center the coordinate system so the offsets equal
    // half the board size.
    [UnityTest]
    public IEnumerator RandomGeneration_ComputesCenterOffsets()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(3, 3));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();

        Board board = gen.Generate(map);
        yield return null;

        Assert.AreEqual(1, board.qOffset);
        Assert.AreEqual(1, board.rOffset);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }

    // Boards loaded from JSON start with cube coordinates beginning at zero, so
    // offsets should be (0,0).
    [UnityTest]
    public IEnumerator LoadBoardFromJson_UsesZeroOffsets()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(2, 2));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        map.PlayArea = gen.Generate(map);

        var saveGO = new GameObject("SaveLoad");
        var slm = saveGO.AddComponent<SaveLoadManager>();
        yield return null;

        string path = Path.Combine(Application.persistentDataPath, "offset_test.json");
        slm.SaveMapToJson(map, path);

        Board loaded = SaveLoadManager.LoadBoardFromJson(path, map, map.transform);
        yield return null;

        Assert.AreEqual(0, loaded.qOffset);
        Assert.AreEqual(0, loaded.rOffset);

        Object.Destroy(genGO);
        Object.Destroy(saveGO);
        Object.Destroy(map.gameObject);
    }

    // Merging two single-tile boards to the starboard side should result in
    // offsets based on the minimum cube coordinates of the merged grid. For a
    // flat topped layout this gives (qOffset=0,rOffset=1).
    [UnityTest]
    public IEnumerator MergeBoards_ComputesOffsets()
    {
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(1, 1));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<GenerateEmptyAir>();
        Board shipA = gen.Generate(map);
        Board shipB = gen.Generate(map);

        MapMerge.MergeBoards(map, shipA, shipB, ShipSide.Starboard);
        yield return null;

        Vector3Int origin = HexUtils.OffsetToCube(Vector2Int.zero, map.isFlatTopped, false);
        Assert.AreEqual(-origin.x, map.PlayArea.qOffset);
        Assert.AreEqual(-origin.y, map.PlayArea.rOffset);

        Assert.AreEqual(2, map.PlayArea._size_X);
        Assert.AreEqual(1, map.PlayArea._size_Y);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }
}
