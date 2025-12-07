// Tests for BoardRotator demonstrating cube coordinate rotations.
// Open Unity’s Test Runner (Window → General → Test Runner) and run Edit Mode tests.
// These tests focus only on the pure RotateCubeCoords method so no GameObjects are required.
/*
 * Cube coordinates (q,r,s) always sum to zero and represent positions on a hex grid.
 * Rotating around the origin means permuting or negating these values.
 * For example starting from (1,0,-1):
 *  - 60° clockwise      -> (1,-1,0)
 *  - 120° clockwise     -> (0,-1,1)
 *  - 180°               -> (-1,0,1)
 *  - 120° counter-clockwise -> (0,1,-1)
 *  - 60° counter-clockwise  -> (-1,1,0)
 * The tests below rotate this sample coordinate and verify each result.
 */

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BoardRotatorTests
{
    // Helper to run rotation on a single coordinate
    private Vector3Int Rotate(Vector3Int coord, BoardRotator.Rotation rot)
    {
        var list = new List<Vector3Int> { coord };
        return BoardRotator.RotateCubeCoords(list, rot)[0];
    }

    [Test]
    public void Rotate60CW_Works()
    {
        // 60° clockwise rotates (q,r,s) -> (-s,-q,-r). For our sample (1,0,-1) that is (1,-1,0).
        var result = Rotate(new Vector3Int(1, 0, -1), BoardRotator.Rotation.Rotate60CW);
        Assert.AreEqual(new Vector3Int(1, -1, 0), result);
    }

    [Test]
    public void Rotate120CW_Works()
    {
        // 120° clockwise rotates (q,r,s) -> (r,s,q). For (1,0,-1) that becomes (0,-1,1).
        var result = Rotate(new Vector3Int(1, 0, -1), BoardRotator.Rotation.Rotate120CW);
        Assert.AreEqual(new Vector3Int(0, -1, 1), result);
    }

    [Test]
    public void Rotate180_Works()
    {
        // 180° rotation simply negates each value: (-q,-r,-s) -> (-1,0,1).
        var result = Rotate(new Vector3Int(1, 0, -1), BoardRotator.Rotation.Rotate180);
        Assert.AreEqual(new Vector3Int(-1, 0, 1), result);
    }

    [Test]
    public void Rotate120CCW_Works()
    {
        // 120° counter-clockwise rotates (q,r,s) -> (-r,-s,-q). (1,0,-1) -> (0,1,-1).
        var result = Rotate(new Vector3Int(1, 0, -1), BoardRotator.Rotation.Rotate120CCW);
        Assert.AreEqual(new Vector3Int(0, 1, -1), result);
    }

    [Test]
    public void Rotate60CCW_Works()
    {
        // 60° counter-clockwise rotates (q,r,s) -> (s,q,r). (1,0,-1) -> (-1,1,0).
        var result = Rotate(new Vector3Int(1, 0, -1), BoardRotator.Rotation.Rotate60CCW);
        Assert.AreEqual(new Vector3Int(-1, 1, 0), result);
    }

    [Test]
    public void RotateNone_ReturnsSame()
    {
        // No rotation should return the original coordinate.
        var coord = new Vector3Int(1, 0, -1);
        var result = Rotate(coord, BoardRotator.Rotation.None);
        Assert.AreEqual(coord, result);
    }
}
