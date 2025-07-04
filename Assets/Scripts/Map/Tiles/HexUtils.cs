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

    public static Vector3Int GetPrimaryDirection(Vector3Int diff)
    {
        // Normalize the direction vector to one of the 6 primary hex directions
        float minDistance = float.MaxValue;
        Vector3Int closest = Vector3Int.zero;

        foreach (var dir in CubeDirections)
        {
            float distance = Vector3Int.Distance(diff, dir);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = dir;
            }
        }

        return closest;
    }

    public static Vector3Int[] GetConeDirections(Vector3Int primary)
    {
        int index = System.Array.IndexOf(CubeDirections, primary);
        if (index == -1) return new Vector3Int[] { };

        Vector3Int left = CubeDirections[(index + 5) % 6];
        Vector3Int right = CubeDirections[(index + 1) % 6];

        return new Vector3Int[] { left, primary, right };
    }

}
