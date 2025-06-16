using UnityEngine;

public static class HexUtils
{
    // Convert Offset Coordinates (Column, Row) to Cube Coordinates (Q, R, S)
    public static Vector3Int OffsetToCube(Vector2Int offset, bool isFlatTopped)
    {
        int col = offset.x;
        int row = offset.y;

        if (isFlatTopped)
        {
            int q = col;
            int r = row - (col + (col & 1)) / 2;
            int s = -q - r;
            return new Vector3Int(q, r, s);
        }
        else
        {
            int q = col - (row + (row & 1)) / 2;
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

    public static Vector2Int CubeToOffset(Vector3Int cube, bool isFlatTopped)
    {
        if (isFlatTopped)
        {
            int col = cube.x;
            int row = cube.z + (cube.x + (cube.x & 1)) / 2;
            return new Vector2Int(col, row);
        }
        else
        {
            int col = cube.x + (cube.z + (cube.z & 1)) / 2;
            int row = cube.z;
            return new Vector2Int(col, row);
        }
    }

}
