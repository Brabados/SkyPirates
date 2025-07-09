using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides pure helper methods for calculating ranges on a hexagonal board.
/// Tiles are addressed using **cube** coordinates (q, r, s) where the three
/// axes always satisfy <c>q + r + s = 0</c>. None of the methods modify the
/// board state which makes them easy to test in isolation.  Extracting the
/// algorithms from <see cref="RangeFinder"/> keeps them free of Unity specific
/// behaviour so unit tests can run without scenes loaded.  AI coding agents can
/// quickly verify logic by calling these methods directly.
///
/// </summary>
public static class RangeCalculator
{
    /// <summary>
    /// Helper used by other algorithms to look up a neighbour by cube offsets
    /// relative to a starting tile.
    /// </summary>
    private static Tile TileAdd(Board board, Tile tile, int q, int r, int s)
    {
        return board.SearchTileByCubeCoordinates(tile.QAxis + q, tile.RAxis + r, tile.SAxis + s);
    }

    /// <summary>
    /// Returns the tile scaled from the origin by <paramref name="factor"/>.
    /// </summary>
    /// <param name="board">Board the tile belongs to.</param>
    /// <param name="tile">Tile to scale from origin.</param>
    /// <param name="factor">Scaling factor.</param>
    /// <returns>Tile at the scaled coordinates or null if out of bounds.</returns>
    public static Tile HexScale(Board board, Tile tile, int factor)
    {
        return board.SearchTileByCubeCoordinates(tile.QAxis * factor, tile.RAxis * factor, tile.SAxis * factor);
    }

    /// <summary>
    /// Calculates all tiles within a hexagonal area ring of <paramref name="radius"/> around <paramref name="center"/>.
    /// </summary>
    /// <param name="board">Board containing the tiles.</param>
    /// <param name="center">Center tile.</param>
    /// <param name="radius">Distance from center.</param>
    /// <returns>List of tiles in the specified ring.</returns>
    public static List<Tile> AreaRing(Board board, Tile center, int radius)
    {
        List<Tile> results = new List<Tile>();

        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                int s = -q - r;
                Tile add = TileAdd(board, center, q, r, s);
                if (add != null)
                {
                    results.Add(add);
                }
            }
        }
        return results;
    }

    /// <summary>
    /// Gets all tiles at exactly <paramref name="radius"/> distance from <paramref name="center"/>.
    /// </summary>
    /// <param name="board">Board containing the tiles.</param>
    /// <param name="center">Starting tile.</param>
    /// <param name="radius">Ring radius.</param>
    /// <returns>List of tiles making up the ring.</returns>
    public static List<Tile> HexRing(Board board, Tile center, int radius)
    {
        List<Tile> results = new List<Tile>();
        if (center == null || radius <= 0) return results;

        Vector3Int cube = new Vector3Int(center.QAxis, center.RAxis, center.SAxis);

        // Start at cube + direction[4] * radius (arbitrary consistent start)
        Vector3Int direction = HexUtils.CubeDirections[4];
        Vector3Int current = cube + direction * radius;

        // Walk around the ring
        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < radius; step++)
            {
                Tile tile = board.SearchTileByCubeCoordinates(current.x, current.y, current.z);
                if (tile != null)
                    results.Add(tile);

                // move to next hex in this side’s direction
                Vector3Int dir = HexUtils.CubeDirections[side];
                current += dir;
            }
        }

        return results;
    }



    /// <summary>
    /// Finds all tiles reachable from <paramref name="start"/> given movement cost.
    /// </summary>
    /// <param name="board">Board containing the tiles.</param>
    /// <param name="start">Starting tile.</param>
    /// <param name="movement">Maximum movement cost.</param>
    /// <returns>List of reachable tiles including the start tile.</returns>
    public static List<Tile> HexReachable(Board board, Tile start, int movement)
    {
        HashSet<Tile> visited = new HashSet<Tile>();
        Queue<(Tile tile, int cost)> fringes = new Queue<(Tile, int)>();

        visited.Add(start);
        fringes.Enqueue((start, 0));

        while (fringes.Count > 0)
        {
            var (currentTile, currentCost) = fringes.Dequeue();

            foreach (Tile neighbor in currentTile.Neighbours)
            {
                if (neighbor != null && !visited.Contains(neighbor) && !(neighbor.Data.MovementCost == 0))
                {
                    int newCost = currentCost + neighbor.Data.MovementCost;
                    if (newCost <= movement && neighbor.Contents == null)
                    {
                        visited.Add(neighbor);
                        fringes.Enqueue((neighbor, newCost));
                    }
                }
            }
        }

        return new List<Tile>(visited);
    }

    /// <summary>
    /// Creates a line of tiles starting from <paramref name="center"/> towards <paramref name="target"/>.
    /// </summary>
    /// <param name="board">Board containing the tiles.</param>
    /// <param name="center">Starting tile.</param>
    /// <param name="target">Tile defining the direction.</param>
    /// <param name="range">Maximum length of the line.</param>
    /// <returns>List of tiles in the line.</returns>
    public static List<Tile> AreaLine(Board board, Tile origin, Tile target, int range)
    {
        List<Tile> line = new List<Tile>();
        if (origin == null || target == null || range <= 0) return line;

        Vector3Int direction = board.GetDirectionVector(origin, target);
        if (direction == Vector3Int.zero) return line;

        Tile current = origin;
        for (int i = 0; i < range; i++)
        {
            current = board.GetNeighbourInDirection(current, direction);
            if (current == null) break;
            line.Add(current);
        }

        return line;
    }



    public static List<Tile> AreaCone(Board board, Tile origin, Tile target, int range, int size)
    {
        List<Tile> result = new List<Tile>();
        if (origin == null || target == null || range <= 0 || size <= 0) return result;

        Vector3Int forward = board.GetDirectionVector(origin, target);
        if (forward == Vector3Int.zero) return result;

        int directionIndex = System.Array.IndexOf(HexUtils.CubeDirections, forward);
        if (directionIndex == -1) return result;

        // Get adjacent directions for spread
        Vector3Int leftDir = HexUtils.CubeDirections[(directionIndex + 5) % 6];
        Vector3Int rightDir = HexUtils.CubeDirections[(directionIndex + 1) % 6];

        for (int depth = 1; depth <= range; depth++)
        {
            // Move to the depth tile in forward direction
            Vector3Int center = new Vector3Int(
                origin.QAxis + forward.x * depth,
                origin.RAxis + forward.y * depth,
                origin.SAxis + forward.z * depth
            );

            // Fan out side-to-side from center at this depth
            for (int spread = -size; spread <= size; spread++)
            {
                Vector3Int offset = CubeLerpOffset(center, leftDir, rightDir, spread);
                Tile tile = board.SearchTileByCubeCoordinates(offset.x, offset.y, offset.z);
                if (tile != null && !result.Contains(tile))
                    result.Add(tile);
            }
        }

        return result;
    }

    // Cube offset interpolation between left/right at given spread distance
    private static Vector3Int CubeLerpOffset(Vector3Int center, Vector3Int left, Vector3Int right, int offset)
    {
        if (offset == 0) return center;
        Vector3Int stepDir = offset < 0 ? left : right;
        offset = Mathf.Abs(offset);
        return new Vector3Int(
            center.x + stepDir.x * offset,
            center.y + stepDir.y * offset,
            center.z + stepDir.z * offset
        );
    }


    // Utility to rotate direction by offset (-2 to +2 means left to right of cone axis)
    private static Vector3Int GetSpreadDirection(Vector3Int forward, int offset)
    {
        if (offset == 0) return forward;

        int index = System.Array.IndexOf(HexUtils.CubeDirections, forward);
        if (index == -1) return forward;

        int spreadIndex = (index + offset + 6) % 6;
        return HexUtils.CubeDirections[spreadIndex];
    }


    public static List<Tile> AreaDiagonal(Board board, Tile center, int range)
    {
        List<Tile> diagonals = new List<Tile>();

        if (center == null || range <= 0) return diagonals;

        // Cube diagonal directions for flat-topped hexes
        Vector3Int[] diagonalDirections = new Vector3Int[]
        {
        new Vector3Int(1, 1, -2),  // Up-Right Diagonal
        new Vector3Int(-1, -1, 2), // Down-Left Diagonal
        new Vector3Int(1, -2, 1),  // Up-Left Diagonal
        new Vector3Int(-1, 2, -1)  // Down-Right Diagonal
        };

        foreach (var direction in diagonalDirections)
        {
            Tile current = center;

            for (int i = 1; i <= range; i++)
            {
                current = board.SearchTileByCubeCoordinates(
                    current.QAxis + direction.x,
                    current.RAxis + direction.y,
                    current.SAxis + direction.z
                );

                if (current != null)
                {
                    diagonals.Add(current);
                }
                else
                {
                    break; // Stop this direction if out of bounds
                }
            }
        }

        return diagonals;
    }

    public static List<Tile> AreaPath(Board board, Tile center, Tile target, int range)
    {
        List<Tile> path = new List<Tile>();

        if (center == null || target == null || range <= 0) return path;

        // Use linear interpolation to create a line between center and target
        Vector3 centerCube = new Vector3(center.QAxis, center.RAxis, center.SAxis);
        Vector3 targetCube = new Vector3(target.QAxis, target.RAxis, target.SAxis);

        for (int i = 1; i <= range; i++)
        {
            float t = (1.0f / (range + 1)) * i;
            Vector3 lerp = Vector3.Lerp(centerCube, targetCube, t);
            Vector3Int rounded = HexUtils.RoundCubeCoordinates(lerp);

            Tile tile = board.SearchTileByCubeCoordinates(rounded.x, rounded.y, rounded.z);
            if (tile != null && !path.Contains(tile))
            {
                path.Add(tile);
            }
        }

        return path;
    }

    public static List<Tile> AreaLineFan(Board board, Tile origin, int range)
    {
        List<Tile> result = new List<Tile>();
        if (origin == null || range <= 0) return result;


        foreach (Vector3Int direction in HexUtils.CubeDirections)
        {
            Tile current = origin;

            for (int i = 0; i < range; i++)
            {
                current = board.GetNeighbourInDirection(current, direction);

                if (current != null && !result.Contains(current))
                {
                    result.Add(current);
                }
                else break;
            }
        }

        return result;
    }



    public static List<Tile> AreaConeFan(Board board, Tile origin, int range, int size)
    {
        List<Tile> result = new List<Tile>();
        if (origin == null || range <= 0 || size <= 0) return result;

        foreach (var forward in HexUtils.CubeDirections)
        {
            int dirIndex = System.Array.IndexOf(HexUtils.CubeDirections, forward);
            Vector3Int leftDir = HexUtils.CubeDirections[(dirIndex + 5) % 6];
            Vector3Int rightDir = HexUtils.CubeDirections[(dirIndex + 1) % 6];

            for (int depth = 1; depth <= range; depth++)
            {
                Vector3Int center = new Vector3Int(
                    origin.QAxis + forward.x * depth,
                    origin.RAxis + forward.y * depth,
                    origin.SAxis + forward.z * depth
                );

                for (int spread = -size; spread <= size; spread++)
                {
                    Vector3Int offset = CubeLerpOffset(center, leftDir, rightDir, spread);
                    Tile tile = board.SearchTileByCubeCoordinates(offset.x, offset.y, offset.z);
                    if (tile != null && !result.Contains(tile))
                        result.Add(tile);
                }
            }
        }

        return result;
    }


}
