using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

/// <summary>
/// PlayMode tests for <see cref="Map.GetHexPositionFromCoordinate"/>.  They
/// illustrate how axial (q,r) grid coordinates map to world positions for both
/// flat topped and pointy topped hex layouts.  The calculations mirror those in
/// <c>Map.GetHexPositionFromCoordinate</c>:
/// <code>
/// width  = sqrt(3) * size
/// height = 2 * size
/// offset = isFlatTopped ? width / 2 : 0
/// x = q * (width * 0.9) + offset
/// z = -(r + q / 2) * height * 0.9
/// </code>
/// When <c>size = 1</c> the following table gives a few sample results used in
/// the assertions below:
/// <code>
/// | q | r | layout | expected (x,z) |
/// | 0 | 0 | flat   | (0.866,  0.000) |
/// | 1 | 0 | flat   | (2.425, -0.900) |
/// | 0 | 0 | pointy | (0.000,  0.000) |
/// | 1 | 1 | pointy | (1.559, -2.700) |
/// </code>
/// </summary>
public class MapCoordinateConversionTests
{
    private Map CreateMap(float innerSize, float outerSize, bool isFlatTopped)
    {
        var go = new GameObject();
        var map = go.AddComponent<Map>();
        map.innerSize = innerSize;
        map.outerSize = outerSize;
        map.isFlatTopped = isFlatTopped;
        return map;
    }

    [Test]
    public void FlatTopped_Positions_AreCalculatedCorrectly()
    {
        var map = CreateMap(innerSize: 0.5f, outerSize: 1f, isFlatTopped: true);

        // Manual calculation when size = 1 (outerSize)
        // width = sqrt(3)*size = 1.73205
        // height = 2*size = 2
        // offset = width/2 = 0.866025

        // q=0,r=0
        // x = 0*(width*0.9)+offset = 0+0.866025
        // z = -((0 + 0/2)*height*0.9) = -0
        Assert.That(map.GetHexPositionFromCoordinate(new Vector2Int(0, 0)),
            Is.EqualTo(new Vector3(0.866025f, 0f, 0f)).Using(Vector3ComparerWithEqualsOperator.Instance));

        // q=1,r=0
        // x = 1*(width*0.9)+offset = 1*(1.73205*0.9)+0.866025 = 2.42487
        // z = -((0 + 1/2)*height*0.9) = -0.9
        Assert.That(map.GetHexPositionFromCoordinate(new Vector2Int(1, 0)),
            Is.EqualTo(new Vector3(2.42487f, 0f, -0.9f)).Using(Vector3ComparerWithEqualsOperator.Instance));
    }

    [Test]
    public void PointyTopped_Positions_AreCalculatedCorrectly()
    {
        var map = CreateMap(innerSize: 0.5f, outerSize: 1f, isFlatTopped: false);

        // Pointy topped hexes have no horizontal offset
        // width = sqrt(3)*size = 1.73205
        // height = 2*size = 2

        // q=0,r=0 -> (0,0,0)
        Assert.That(map.GetHexPositionFromCoordinate(new Vector2Int(0, 0)),
            Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));

        // q=1,r=1
        // x = 1*(width*0.9)+0 = 1*(1.73205*0.9) = 1.55885
        // z = -((1 + 1/2)*height*0.9) = -2.7
        Assert.That(map.GetHexPositionFromCoordinate(new Vector2Int(1, 1)),
            Is.EqualTo(new Vector3(1.55885f, 0f, -2.7f)).Using(Vector3ComparerWithEqualsOperator.Instance));
    }
}
