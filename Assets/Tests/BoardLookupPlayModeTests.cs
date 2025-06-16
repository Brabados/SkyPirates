// PlayMode tests verifying board lookup consistency.
// --------------------------------------------------
// Open Unity's Test Runner (Window > General > Test Runner) and run the
// PlayMode suite to execute these tests.
//
// Each test generates a board through a different code path (random
// generation, save/load, merging, rotation) and then checks that every
// tile can be retrieved using its cube coordinates.  These lookups are
// fundamental for gameplay and should remain constant time.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardLookupPlayModeTests
{
    /// <summary>
    /// Creates a bare bones <see cref="Map"/> with two simple tile types.
    /// These tests do not rely on any real game data, so we generate
    /// minimal scriptable objects at runtime.
    /// </summary>
    private Map CreateBasicMap(Vector2Int size)
    {
        var go = new GameObject("Map");
        var map = go.AddComponent<Map>();
        map.enabled = false; // don't auto-run Start
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

        // Add required components
        var generator = go.AddComponent<RandomGeneration>();
        go.AddComponent<MovementLine>();

        // Manual setup
        map.generate = generator;
        map.PlayArea = generator.Generate(map);
        map.SetNeighbours(map.PlayArea, map.isFlatTopped);

        return map;
    }


    /// <summary>
    /// Many of the generation utilities rely on a scene containing a
    /// Camera with a <see cref="CameraController"/>.  PlayMode tests start
    /// with an empty scene, so we spawn one if needed.
    /// </summary>
    private void EnsureCamera()
    {
        var camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        var camera = camGO.AddComponent<Camera>();
        var controller = camGO.AddComponent<CameraController>();

        // Create a dummy child transform to act as the zoom target
        var zoomTransform = new GameObject("CameraPivot").transform;
        zoomTransform.parent = camGO.transform;
        zoomTransform.localPosition = new Vector3(0, 10, -10);

        controller.cameraTransform = zoomTransform;

        controller.movementSpeedNormal = 5f;
        controller.movementSpeedFast = 10f;
        controller.movementTime = 5f;
        controller.rotationAmount = 1f;
        controller.minimumZoom = 5f;
        controller.maximumZoom = 20f;
        controller.zoomAmount = new Vector3(0, -1, 1);
    }

    /// <summary>
    /// Helper used by each test to verify that searching the board by cube
    /// coordinates returns the exact same tile instance that we started with.
    /// </summary>
    private void AssertBoardLookups(Board board)
    {
        foreach (var tile in board.GetAllTiles())
        {
            Vector3Int cube = new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis);
            Assert.AreSame(tile, board.GetTileByCube(cube));
            Assert.AreSame(tile, board.SearchTileByCubeCoordinates(cube));

        }
    }

    /// <summary>
    /// Verifies tiles can be found after randomly generating a board.
    /// </summary>
    [UnityTest]
    public IEnumerator RandomGenerationBoardLookup()
    {
        // Arrange - create a simple map and generator
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(3, 3));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();

        // Act - generate a random board
        Board board = gen.Generate(map);
        yield return null; // wait a frame for any coroutines

        // Assert - every tile must be reachable via cube coordinates
        AssertBoardLookups(board);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }

    /// <summary>
    /// Saves a generated board to JSON and then loads it back to ensure the
    /// lookup structures are preserved in the serialized data.
    /// </summary>
    [UnityTest]
    public IEnumerator LoadBoardFromJsonLookup()
    {
        // Arrange - create a map then save it to disk
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(2, 2));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        map.PlayArea = gen.Generate(map);
        var saveGO = new GameObject("SaveLoad");
        var slm = saveGO.AddComponent<SaveLoadManager>();
        yield return null; // allow components to initialise

        string path = Path.Combine(Application.persistentDataPath, "temp_board.json");
        slm.SaveMapToJson(map, path);

        // Act - load a board back from the saved file
        Board loaded = SaveLoadManager.LoadBoardFromJson(path, map, map.transform);
        yield return null;

        // Assert
        AssertBoardLookups(loaded);

        Object.Destroy(genGO);
        Object.Destroy(saveGO);
        Object.Destroy(map.gameObject);
    }

    /// <summary>
    /// Merges two boards together and checks that the resulting board can
    /// still look up tiles correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator MergeBoardsLookup()
    {
        // Arrange - build two boards we can merge
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(2, 2));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        Board a = gen.Generate(map);
        Board b = gen.Generate(map);

        // Act - merge the boards together
        MapMerge.MergeBoards(map, a, b, ShipSide.Bow);
        yield return null;

        // Assert
        AssertBoardLookups(map.PlayArea);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }

    /// <summary>
    /// Rotates an existing board and confirms that rotated tiles maintain
    /// valid cube coordinate lookups.
    /// </summary>
    [UnityTest]
    public IEnumerator RotateBoardLookup()
    {
        // Arrange - generate a board we can rotate
        EnsureCamera();
        var map = CreateBasicMap(new Vector2Int(3, 3));
        var genGO = new GameObject("Gen");
        var gen = genGO.AddComponent<RandomGeneration>();
        Board board = gen.Generate(map);

        // Act - rotate the board 60 degrees clockwise
        Board rotated = BoardRotator.RotateBoard(board, BoardRotator.Rotation.Rotate60CW);
        yield return null;

        // Assert
        AssertBoardLookups(rotated);

        Object.Destroy(genGO);
        Object.Destroy(map.gameObject);
    }
}
