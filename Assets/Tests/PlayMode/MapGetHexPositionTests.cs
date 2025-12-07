using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public class MapGetHexPositionTests
{
    private Map CreateMap(float innerSize, float outerSize, bool isFlatTopped)
    {
        var go = new GameObject("Map");
        var map = go.AddComponent<Map>();
        map.MapSize = new Vector2Int(1, 1);
        map.innerSize = innerSize;
        map.outerSize = outerSize;
        map.isFlatTopped = isFlatTopped;
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

        return map;
    }

    /*
        These tests walk through the math used by Map.GetHexPositionFromCoordinate.
        They document the standard axial-to-world formulas which replace the
        old width/2 offset method.  The previous approach misaligned the origin
        for flat topped boards and produced unexpected positions.
        The grid uses axial coordinates (q,r).  Below is a small diagram of the
        coordinate system used in the tests:

               (-1,1)  (0,1)  (1,1)
                  \      |      /
               (-1,0) (0,0) (1,0)
                  /      |      \
               (-1,-1)(0,-1)(1,-1)

        For flat topped layouts the pixel coordinates are:
            x = size * 3/2 * q
            z = size * sqrt(3) * (r + q/2)

        For pointy topped layouts the pixel coordinates are:
            x = size * sqrt(3) * (q + r/2)
            z = size * 3/2 * r

        Each test below shows the manual calculation for a given (q,r) pair
        with an additional 0.9 spacing factor applied.
    */
    [Test]
    public void FlatTopped_GetHexPosition_ReturnsExpectedValues()
    {
        // Arrange: create a map using axial coordinates with known sizes
        var map = CreateMap(innerSize: 0.5f, outerSize: 1f, isFlatTopped: true);

        // Manual calculation for q=0,r=0 when flat topped:
        // x = 1.5*q*size*0.9 = 0
        // z = sqrt(3)*(r + q/2)*size*0.9 = 0
        Assert.That(map.GetHexPositionFromCoordinate(new Vector2Int(0, 0)),
            Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));

        // Small table of expected positions (size=1, gap=0.9):
        // | q | r | (x,z)         |
        // | 0 | 0 | (0.000, 0.000)|
        // | 1 | 0 | (1.350,-0.779)|
        // | 0 | 1 | (0.000,-1.559)|

        Vector3 expected1 = new Vector3(1.35f, 0f, -0.77942286f);
        Vector3 actual1 = map.GetHexPositionFromCoordinate(new Vector2Int(1, 0));
        Assert.That(actual1, Is.EqualTo(expected1).Using(Vector3ComparerWithEqualsOperator.Instance));

        Vector3 expected2 = new Vector3(0f, 0f, -1.5588458f);
        Vector3 actual2 = map.GetHexPositionFromCoordinate(new Vector2Int(0, 1));
        Assert.That(actual2, Is.EqualTo(expected2).Using(Vector3ComparerWithEqualsOperator.Instance));
    }

    // Negative coordinates should work too. This helps ensure that the formula
    // does not rely on clamped values.
    [Test]
    public void FlatTopped_NegativeCoordinates_AreCalculatedCorrectly()
    {
        var map = CreateMap(innerSize: 0.5f, outerSize: 1f, isFlatTopped: true);

        // q=-1, r=0 -> x=1.5*-1*size*0.9=-1.35, z=sqrt(3)*(-0.5)*size*0.9=0.779
        Vector3 expected = new Vector3(-1.35f, 0f, 0.77942286f);
        Vector3 actual = map.GetHexPositionFromCoordinate(new Vector2Int(-1, 0));
        Assert.That(actual, Is.EqualTo(expected).Using(Vector3ComparerWithEqualsOperator.Instance));
    }

    // The same checks for a pointy topped layout. Here the origin is exactly at
    // (0,0,0) which can make grid math easier to reason about.
    [Test]
    public void PointyTopped_GetHexPosition_ReturnsExpectedValues()
    {
        var map = CreateMap(innerSize: 0.5f, outerSize: 1f, isFlatTopped: false);

        // For pointy topped offset=0
        // q=0,r=0 -> (0,0,0)
        Assert.That(map.GetHexPositionFromCoordinate(new Vector2Int(0, 0)),
            Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));

        // Small table of expected positions (size=1, gap=0.9):
        // | q | r | (x,z)         |
        // | 1 | 0 | (1.559, 0.000)|
        // | 1 | 1 | (2.338,-1.350)|

        Vector3 expected1 = new Vector3(1.5588458f, 0f, 0f);
        Vector3 actual1 = map.GetHexPositionFromCoordinate(new Vector2Int(1, 0));
        Assert.That(actual1, Is.EqualTo(expected1).Using(Vector3ComparerWithEqualsOperator.Instance));

        Vector3 expected2 = new Vector3(2.3382686f, 0f, -1.35f);
        Vector3 actual2 = map.GetHexPositionFromCoordinate(new Vector2Int(1, 1));
        Assert.That(actual2, Is.EqualTo(expected2).Using(Vector3ComparerWithEqualsOperator.Instance));

    }

    // The origin (0,0) should map to the world origin for flat topped layouts.
    [Test]
    public void FlatTopped_Origin_IsAtZero()
    {
        var map = CreateMap(innerSize: 0.5f, outerSize: 1f, isFlatTopped: true);

        Assert.That(
            map.GetHexPositionFromCoordinate(new Vector2Int(0, 0)),
            Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));
    }
}
