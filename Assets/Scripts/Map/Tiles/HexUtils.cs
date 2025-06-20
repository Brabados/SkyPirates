using UnityEngine;

public static class HexUtils
{

    public static Vector3Int OffsetToCube(Vector2Int offset, bool isFlatTopped,
                                          bool useOdd = false)
    {
        int col = offset.x;
        int row = offset.y;

        if (isFlatTopped)
        {
            // Flat topped hexes use column based (q) offsets
            int q = col;
            int r = useOdd
                ? row - (col - (col & 1)) / 2  // odd-q
                : row - (col + (col & 1)) / 2; // even-q
            int s = -q - r;
            return new Vector3Int(q, r, s);
        }
        else
        {
            // Pointy topped hexes use row based (r) offsets
            int q = useOdd
                ? col - (row - (row & 1)) / 2   // odd-r
                : col - (row + (row & 1)) / 2;  // even-r
            int r = row;
            int s = -q - r;
            return new Vector3Int(q, r, s);
        }
    }

    // 6 Hex Directions in Cube Coordinates
    public static readonly Vector3Int[] CubeDirections = new Vector3Int[]
    {
        new Vector3Int(1, -1, 0),
        new Vector3Int(1, 0, -1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(0, -1, 1)
    };
}
