using NUnit.Framework;
using UnityEngine;

public class HexUtilsOffsetParityTests
{
    /*
        HexUtils.OffsetToCube converts (column,row) offset coordinates
        into cube coordinates used by the rest of the game.  The
        conversion depends on the orientation of the grid and whether
        the layout uses even or odd parity.

        Below are tiny diagrams for the four supported layouts.  Each
        diagram shows how the coordinates (0,0) and (1,0) relate when
        the second column or row is shifted.

        even-q (flat topped):
            (0,1) (1,1)
        (0,0) (1,0)
            \ shift down on even columns

        odd-q (flat topped):
        (0,1) (1,1)
            (0,0) (1,0)
            \ shift down on odd columns

        even-r (pointy topped):
            (0,1)
        (0,0) (1,1)
         shift right on even rows

        odd-r (pointy topped):
        (0,1)
            (0,0) (1,1)
         shift right on odd rows
    */

    [Test]
    public void OffsetToCube_FlatTopped_EvenQ()
    {
        // even-q shifts column 1 downward
        Assert.AreEqual(new Vector3Int(0, 0, 0),
            HexUtils.OffsetToCube(new Vector2Int(0, 0), true, false));
        Assert.AreEqual(new Vector3Int(1, -1, 0),
            HexUtils.OffsetToCube(new Vector2Int(1, 0), true, false));
        Assert.AreEqual(new Vector3Int(1, 0, -1),
            HexUtils.OffsetToCube(new Vector2Int(1, 1), true, false));
    }

    [Test]
    public void OffsetToCube_FlatTopped_OddQ()
    {
        // odd-q shifts column 1 upward instead
        Assert.AreEqual(new Vector3Int(0, 0, 0),
            HexUtils.OffsetToCube(new Vector2Int(0, 0), true, true));
        Assert.AreEqual(new Vector3Int(1, 0, -1),
            HexUtils.OffsetToCube(new Vector2Int(1, 0), true, true));
        Assert.AreEqual(new Vector3Int(1, 1, -2),
            HexUtils.OffsetToCube(new Vector2Int(1, 1), true, true));
    }

    [Test]
    public void OffsetToCube_PointyTopped_EvenR()
    {
        // even-r shifts row 1 to the right
        Assert.AreEqual(new Vector3Int(0, 0, 0),
            HexUtils.OffsetToCube(new Vector2Int(0, 0), false, false));
        Assert.AreEqual(new Vector3Int(-1, 1, 0),
            HexUtils.OffsetToCube(new Vector2Int(0, 1), false, false));
        Assert.AreEqual(new Vector3Int(0, 1, -1),
            HexUtils.OffsetToCube(new Vector2Int(1, 1), false, false));
    }

    [Test]
    public void OffsetToCube_PointyTopped_OddR()
    {
        // odd-r shifts row 1 to the left instead
        Assert.AreEqual(new Vector3Int(0, 0, 0),
            HexUtils.OffsetToCube(new Vector2Int(0, 0), false, true));
        Assert.AreEqual(new Vector3Int(0, 1, -1),
            HexUtils.OffsetToCube(new Vector2Int(0, 1), false, true));
        Assert.AreEqual(new Vector3Int(1, 1, -2),
            HexUtils.OffsetToCube(new Vector2Int(1, 1), false, true));
    }
}
