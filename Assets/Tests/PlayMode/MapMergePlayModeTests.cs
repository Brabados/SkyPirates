using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

/*
    Play mode tests covering the MapMerge utility. The diagrams below illustrate
    how board B is positioned relative to board A when merging:

        Bow            Stern
         B              A
         A              B

        Port           Starboard
        B A             A B

    Coordinates shown are offset indices (x to the right, y up). The tests below
    merge small boards using all sides and verify resulting offsets, cube
    coordinates and that FillNulls populates empty spaces.
*/

public class MapMergePlayModeTests
{
    private Map CreateBasicMap(bool withGenerator = false)
    {
        var go = new GameObject("Map");
        var map = go.AddComponent<Map>();
        map.MapSize = new Vector2Int(2, 2);
        map.innerSize = 0.5f;
        map.outerSize = 1f;
        map.isFlatTopped = true;
        map.TileTypes = new List<TileDataSO>();

        // Create dummy mesh
        Mesh dummyMesh = new Mesh();
        dummyMesh.vertices = new Vector3[] {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, 1)
    };
        dummyMesh.triangles = new int[] { 0, 1, 2 };

        GameObject prefab1 = new GameObject("prefab1");
        prefab1.AddComponent<MeshFilter>().mesh = dummyMesh;
        prefab1.AddComponent<MeshRenderer>();
        prefab1.AddComponent<MeshCollider>().sharedMesh = dummyMesh;

        var tile1 = ScriptableObject.CreateInstance<TileDataSO>();
        tile1.UniqueID = "type1";
        tile1.TilePrefab = prefab1;
        tile1.BaseMat = new Material(Shader.Find("Standard"));
        map.TileTypes.Add(tile1);

        var tile2 = ScriptableObject.CreateInstance<TileDataSO>();
        tile2.UniqueID = "type2";
        tile2.TilePrefab = prefab1; // reuse the same dummy mesh
        tile2.BaseMat = tile1.BaseMat;
        map.TileTypes.Add(tile2);

        if (withGenerator)
        {
            map.generate = go.AddComponent<GenerateEmptyAir>();
            map.PlayArea = map.generate.Generate(map);
        }

        return map;
    }


    private Tile CreateTile(Vector2Int coords, Vector2Int boardSize, Map map, TileDataSO data)
    {


        GameObject go = new GameObject($"Tile_{coords.x}_{coords.y}");

        // Add required components
        Tile tile = go.AddComponent<Tile>();
        HexRenderer hexRenderer = go.AddComponent<HexRenderer>();
        MeshCollider collider = go.AddComponent<MeshCollider>();

        tile.Data = data;

        float height = data == map.TileTypes[0] ? 5f : 20f;
        tile.SetPositionAndHeight(coords, coords.x, coords.y, height);

        // Setup HexRenderer before Start() is called
        tile.SetupHexRenderer(map.innerSize, map.outerSize, map.isFlatTopped);

        // Manually simulate what Start() would do to prevent nulls
        tile.Hex.DrawMesh();             // this sets .H_ColiderMesh
        tile.Hex.GetColliderMesh();
        tile.SetColliderMesh();          // now safe – Hex.H_ColiderMesh is valid

        // Final positioning
        Vector3 pos = map.GetHexPositionFromCoordinate(coords);
        pos.y += height / 2f;
        go.transform.position = pos;
        go.transform.SetParent(map.transform);

        return tile;
    }

    private Board CreateBoard(Vector2Int size, Map map, TileDataSO data)
    {
        Board board = new Board(size);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Tile tile = CreateTile(new Vector2Int(x, y), size, map, data);
                board.set_Tile(x, y, tile);
            }
        }
        return board;
    }

    [UnityTest]
    public IEnumerator MergeBoards_TileOffsetsCorrect([
        Values(ShipSide.Bow, ShipSide.Stern, ShipSide.Port, ShipSide.Starboard)] ShipSide side)
    {
        var map = CreateBasicMap();
        var boardA = CreateBoard(new Vector2Int(1, 1), map, map.TileTypes[1]);
        var boardB = CreateBoard(new Vector2Int(1, 1), map, map.TileTypes[1]);

        var layout = MapMerge.GetMergeLayout(new Vector2Int(1, 1), new Vector2Int(1, 1), side);
        MapMerge.MergeBoards(map, boardA, boardB, side);
        yield return null;

        Assert.AreEqual(layout.merged.x, map.PlayArea._size_X);
        Assert.AreEqual(layout.merged.y, map.PlayArea._size_Y);

        Tile tileA = map.PlayArea.get_Tile(layout.offsetA.x, layout.offsetA.y);
        Tile tileB = map.PlayArea.get_Tile(layout.offsetB.x, layout.offsetB.y);
        Assert.NotNull(tileA);
        Assert.NotNull(tileB);
        Assert.AreEqual(map.TileTypes[1], tileA.Data);
        Assert.AreEqual(map.TileTypes[1], tileB.Data);

        Vector3Int cubeA = HexUtils.OffsetToCube(layout.offsetA, map.isFlatTopped);
        Vector3Int cubeB = HexUtils.OffsetToCube(layout.offsetB, map.isFlatTopped);
        Assert.AreEqual(cubeA.x, tileA.QAxis);
        Assert.AreEqual(cubeA.y, tileA.RAxis);
        Assert.AreEqual(cubeA.z, tileA.SAxis);
        Assert.AreEqual(cubeB.x, tileB.QAxis);
        Assert.AreEqual(cubeB.y, tileB.RAxis);
        Assert.AreEqual(cubeB.z, tileB.SAxis);

        Object.DestroyImmediate(map.gameObject);
    }

    // Merge boards of different sizes on both flat and pointy topped maps.
    // Each orientation is checked to ensure offsets match GetMergeLayout.
    [UnityTest]
    public IEnumerator MergeBoards_VaryingSizes(
        [Values(ShipSide.Bow, ShipSide.Stern, ShipSide.Port, ShipSide.Starboard)] ShipSide side,
        [Values(true, false)] bool isFlat)
    {
        var map = CreateBasicMap();
        map.isFlatTopped = isFlat;

        var sizeA = new Vector2Int(2, 1);
        var sizeB = new Vector2Int(1, 2);
        var boardA = CreateBoard(sizeA, map, map.TileTypes[1]);
        var boardB = CreateBoard(sizeB, map, map.TileTypes[1]);

        var layout = MapMerge.GetMergeLayout(sizeA, sizeB, side);
        MapMerge.MergeBoards(map, boardA, boardB, side);
        yield return null;

        Assert.AreEqual(layout.merged.x, map.PlayArea._size_X);
        Assert.AreEqual(layout.merged.y, map.PlayArea._size_Y);

        // Spot check a corner tile from each board using the computed offsets.
        Tile aCorner = map.PlayArea.get_Tile(layout.offsetA.x, layout.offsetA.y);
        Tile bCorner = map.PlayArea.get_Tile(layout.offsetB.x + sizeB.x - 1, layout.offsetB.y + sizeB.y - 1);

        Assert.NotNull(aCorner);
        Assert.NotNull(bCorner);
        Assert.AreEqual(map.TileTypes[1], aCorner.Data);
        Assert.AreEqual(map.TileTypes[1], bCorner.Data);

        Vector3Int cubeA = HexUtils.OffsetToCube(new Vector2Int(layout.offsetA.x, layout.offsetA.y), map.isFlatTopped);
        Vector3Int cubeB = HexUtils.OffsetToCube(new Vector2Int(layout.offsetB.x + sizeB.x - 1, layout.offsetB.y + sizeB.y - 1), map.isFlatTopped);

        Assert.AreEqual(cubeA, new Vector3Int(aCorner.QAxis, aCorner.RAxis, aCorner.SAxis));
        Assert.AreEqual(cubeB, new Vector3Int(bCorner.QAxis, bCorner.RAxis, bCorner.SAxis));

        Object.DestroyImmediate(map.gameObject);
    }

    [Test]
    public void FillNulls_PopulatesMissingTiles()
    {
        var map = CreateBasicMap();
        Board board = new Board(new Vector2Int(2, 2));
        Tile existing = CreateTile(Vector2Int.zero, new Vector2Int(2, 2), map, map.TileTypes[1]);
        board.set_Tile(0, 0, existing);

        MapMerge.FillNulls(board, map);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Tile t = board.get_Tile(x, y);
                Assert.NotNull(t);

                Vector2Int offsetCoords = new Vector2Int(x, y);
                Vector3Int expectedCube = HexUtils.OffsetToCube(offsetCoords, map.isFlatTopped, false);

                // Print actual
                Debug.Log($"At {offsetCoords} expectedCube: {expectedCube}, actual: ({t.QAxis},{t.RAxis},{t.SAxis})");


                Assert.AreEqual(expectedCube.x, t.QAxis);
                Assert.AreEqual(expectedCube.y, t.RAxis);
                Assert.AreEqual(expectedCube.z, t.SAxis);

                Vector3 expectedPos = map.GetHexPositionFromCoordinate(new Vector2Int(x, y));
                expectedPos.y += t.Height / 2f;
                Assert.That(t.transform.position, Is.EqualTo(expectedPos).Using(Vector3ComparerWithEqualsOperator.Instance));

                if (x != 0 || y != 0)
                {
                    Assert.AreEqual(map.TileTypes[0], t.Data);
                    Assert.AreEqual(5f, t.Height);
                }
            }
        }

        Object.DestroyImmediate(map.gameObject);
    }

    // Ensure FillNulls does not overwrite existing tiles while filling blanks.
    [Test]
    public void FillNulls_PreservesExistingTiles()
    {
        var map = CreateBasicMap();
        Board board = new Board(new Vector2Int(2, 2));

        // Pre-place a tile that should remain unchanged.
        Tile existing = CreateTile(new Vector2Int(1, 1), new Vector2Int(2, 2), map, map.TileTypes[1]);
        board.set_Tile(1, 1, existing);

        MapMerge.FillNulls(board, map);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                Tile t = board.get_Tile(x, y);
                Assert.NotNull(t);

                if (x == 1 && y == 1)
                {
                    Assert.AreEqual(existing, t); // Should be the same instance
                    Assert.AreEqual(map.TileTypes[1], t.Data);
                    Assert.AreEqual(20f, t.Height);
                    continue;
                }

                Assert.AreEqual(map.TileTypes[0], t.Data);
                Assert.AreEqual(5f, t.Height);
            }
        }

        Object.DestroyImmediate(map.gameObject);
    }
}

