using System.Collections.Generic;
using UnityEngine;

public static class BoardRotator
{
    public enum Rotation
    {
        None,
        Rotate60CW,
        Rotate120CW,
        Rotate180,
        Rotate120CCW,
        Rotate60CCW
    }

    public static Board RotateBoard(Board original, Rotation rotation)
    {
        List<Tile> originalTiles = new List<Tile>(original.GetAllTiles());
        List<Vector3Int> coords = new List<Vector3Int>();

        // Gather cube coordinates for the board before instantiating any tiles
        foreach (Tile tile in originalTiles)
        {
            coords.Add(new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis));
        }

        // Rotate coordinates using the pure helper so tests don't touch GameObjects
        List<Vector3Int> rotatedCoords = RotateCubeCoords(coords, rotation);

        // Step 2: Compute bounding box for new board size
        int minQ = int.MaxValue, maxQ = int.MinValue;
        int minR = int.MaxValue, maxR = int.MinValue;

        foreach (var coord in rotatedCoords)
        {
            if (coord.x < minQ) minQ = coord.x;
            if (coord.x > maxQ) maxQ = coord.x;
            if (coord.y < minR) minR = coord.y;
            if (coord.y > maxR) maxR = coord.y;
        }

        int width = maxQ - minQ + 1;
        int height = maxR - minR + 1;

        Board rotatedBoard = new Board(new Vector2Int(width, height));

        // Step 3: Instantiate rotated tiles and set them into new board
        for (int i = 0; i < originalTiles.Count; i++)
        {
            Tile oldTile = originalTiles[i];
            Vector3Int rotated = rotatedCoords[i];

            int offsetX = rotated.x - minQ;
            int offsetY = rotated.y - minR;

            Tile newTile = Object.Instantiate(oldTile);
            newTile.name = oldTile.name + $"_rotated_{rotation}";
            newTile.SetQUSPosition(rotated);
            newTile.SetHeight(oldTile.Height);
            newTile.SetPosition(new Vector2Int(offsetX, offsetY));

            rotatedBoard.set_Tile(offsetX, offsetY, newTile);
        }

        // Step 4: Clean up the original board
        original.Destroy();

        return rotatedBoard;
    }

    // Rotate a list of cube coordinates according to the selected rotation.
    // This method is pure and does not touch any GameObjects so it can be used
    // directly in unit tests.
    public static List<Vector3Int> RotateCubeCoords(List<Vector3Int> coords, Rotation rot)
    {
        List<Vector3Int> rotated = new List<Vector3Int>(coords.Count);
        foreach (Vector3Int c in coords)
        {
            rotated.Add(RotateCube(c.x, c.y, c.z, rot));
        }
        return rotated;
    }




    private static Vector3Int RotateCube(int q, int r, int s, Rotation rot)
    {
        // Example rotations for cube coordinate (1,0,-1):
        //  None          -> (1, 0, -1)
        //  Rotate60CW    -> (1, -1, 0)
        //  Rotate120CW   -> (0, -1, 1)
        //  Rotate180     -> (-1, 0, 1)
        //  Rotate120CCW  -> (0, 1, -1)
        //  Rotate60CCW   -> (-1, 1, 0)
        return rot switch
        {
            Rotation.Rotate60CW => new Vector3Int(-s, -q, -r),    // (q,r,s) -> (-s,-q,-r)
            Rotation.Rotate120CW => new Vector3Int(r, s, q),      // (q,r,s) -> (r,s,q)
            Rotation.Rotate180 => new Vector3Int(-q, -r, -s),     // (q,r,s) -> (-q,-r,-s)
            Rotation.Rotate120CCW => new Vector3Int(-r, -s, -q),  // (q,r,s) -> (-r,-s,-q)
            Rotation.Rotate60CCW => new Vector3Int(s, q, r),      // (q,r,s) -> (s,q,r)
            _ => new Vector3Int(q, r, s),                         // No rotation
        };
    }
}
