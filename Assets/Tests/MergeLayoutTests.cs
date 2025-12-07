using NUnit.Framework;
using UnityEngine;

public class MergeLayoutTests
{
    /*
        These tests illustrate how GetMergeLayout positions two boards.
        The diagrams use offsets where (0,0) is the bottom left of the
        merged grid.  Each 'A' or 'B' cell represents a tile from the
        corresponding board.

        Bow attachment (ship B above ship A, both 2x2):

            y
            3  B B
            2  B B
            1  A A
            0  A A
            0 1 2 x

        Expected: shipA offset=(0,0), shipB offset=(0,2), merged=(2,4)
    */
    [Test]
    public void Bow_MergesVertically()
    {
        Vector2Int sizeA = new Vector2Int(2, 2);
        Vector2Int sizeB = new Vector2Int(2, 2);

        var result = MapMerge.GetMergeLayout(sizeA, sizeB, ShipSide.Bow);

        Assert.AreEqual(Vector2Int.zero, result.offsetA);
        Assert.AreEqual(new Vector2Int(0, 2), result.offsetB);
        Assert.AreEqual(new Vector2Int(2, 4), result.merged);
    }

    /*
        Starboard attachment places ship B to the right of ship A.
        Example with ship A size (2x3) and ship B size (1x1):

            y
            2  A A B
            1  A A B
            0  A A B
            0 1 2 3 x

        Expected: shipA offset=(0,0), shipB offset=(2,0), merged=(3,3)
    */
    [Test]
    public void Starboard_MergesHorizontally()
    {
        Vector2Int sizeA = new Vector2Int(2, 3);
        Vector2Int sizeB = new Vector2Int(1, 1);

        var result = MapMerge.GetMergeLayout(sizeA, sizeB, ShipSide.Starboard);

        Assert.AreEqual(Vector2Int.zero, result.offsetA);
        Assert.AreEqual(new Vector2Int(2, 0), result.offsetB);
        Assert.AreEqual(new Vector2Int(3, 3), result.merged);
    }
}

