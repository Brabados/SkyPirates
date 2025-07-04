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

        if (radius == 0)
        {
            return results;
        }

        Tile hex = HexScale(board, center.Neighbours[0], radius);

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                if (hex != null)
                {
                    results.Add(hex);
                    hex = hex.Neighbours[i];
                }
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
    public static List<Tile> AreaLine(Board board, Tile center, Tile target, int range)
    {
        List<Tile> line = new List<Tile>();

        if (center == null || target == null || range <= 0) return line;

        Vector3Int direction = board.GetDirectionVector(center, target);
        if (direction == Vector3Int.zero) return line;

        Tile current = center;

        for (int i = 1; i <= range; i++)
        {
            current = board.GetNeighbourInDirection(current, direction);
            if (current != null)
            {
                line.Add(current);
            }
            else break;
        }

        return line;
    }


    public static List<Tile> AreaCone(Board board, Tile center, Tile target, int range)
    {
        List<Tile> cone = new List<Tile>();
        if (center == null || target == null || range <= 0) return cone;

        // Step 1: Find the primary direction using existing method
        Vector3Int primaryDirection = board.GetDirectionVector(center, target);
        if (primaryDirection == Vector3Int.zero) return cone;

        // Step 2: Get spread directions for the cone
        Vector3Int[] coneDirections = HexUtils.GetConeDirections(primaryDirection);

        // Step 3: Build the cone
        Tile current = center;
        for (int i = 1; i <= range; i++)
        {
            foreach (var coneDir in coneDirections)
            {
                Tile step = current;
                for (int k = 0; k < i; k++)
                {
                    if (step == null) break;
                    step = board.GetNeighbourInDirection(step, coneDir);
                }
                if (step != null && !cone.Contains(step))
                    cone.Add(step);
            }

            current = board.GetNeighbourInDirection(current, primaryDirection);
            if (current == null) break;
        }

        return cone;
    }

}
