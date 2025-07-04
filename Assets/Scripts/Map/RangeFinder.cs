using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// MonoBehaviour wrapper that delegates range calculations to
/// <see cref="RangeCalculator"/>. The heavy math was extracted into that static
/// class so the logic can be tested without requiring a running scene.
/// Keeping this behaviour separate also lets AI coding agents analyse and
/// extend the algorithms more easily.
/// </summary>
public class RangeFinder : MonoBehaviour
{
    /// <summary>
    /// Reference to the <see cref="Map"/> component containing the active board.
    /// If not assigned in the inspector it will be located at runtime.
    /// </summary>
    [SerializeField]
    public Map _GameBoard;

    void Awake()
    {
        if (_GameBoard == null)
        {
            _GameBoard = FindObjectOfType<Map>();
        }
    }

    /// <summary>
    /// Wrapper for <see cref="RangeCalculator.HexScale"/> using the active board.
    /// </summary>
    /// <param name="tile">Tile to scale from the origin.</param>
    /// <param name="factor">Scaling factor applied to the tile's cube coordinates.</param>
    /// <returns>The tile found at the scaled position or <c>null</c> if out of range.</returns>
    public Tile HexScale(Tile tile, int factor)
    {
        return RangeCalculator.HexScale(_GameBoard.PlayArea, tile, factor);
    }

    /// <summary>
    /// Wrapper for <see cref="RangeCalculator.AreaRing"/>.
    /// </summary>
    /// <param name="center">Center tile.</param>
    /// <param name="radius">Distance from the center tile.</param>
    /// <returns>All tiles contained in the specified ring.</returns>
    public List<Tile> AreaRing(Tile center, int radius)
    {
        return RangeCalculator.AreaRing(_GameBoard.PlayArea, center, radius);
    }

    /// <summary>
    /// Wrapper for <see cref="RangeCalculator.HexRing"/>.
    /// </summary>
    /// <param name="center">Tile at the centre of the ring.</param>
    /// <param name="radius">Radius of the ring.</param>
    /// <returns>List of tiles exactly <paramref name="radius"/> away from <paramref name="center"/>.</returns>
    public List<Tile> HexRing(Tile center, int radius)
    {
        return RangeCalculator.HexRing(_GameBoard.PlayArea, center, radius);
    }

    /// <summary>
    /// Wrapper for <see cref="RangeCalculator.HexReachable"/>.
    /// </summary>
    /// <param name="start">Starting tile.</param>
    /// <param name="movement">Maximum movement budget.</param>
    /// <returns>Tiles that can be reached from <paramref name="start"/>.</returns>
    public List<Tile> HexReachable(Tile start, int movement)
    {
        return RangeCalculator.HexReachable(_GameBoard.PlayArea, start, movement);
    }

    /// <summary>
    /// Wrapper for <see cref="RangeCalculator.AreaLine"/>.
    /// </summary>
    /// <param name="center">Starting tile.</param>
    /// <param name="target">Tile determining the direction.</param>
    /// <param name="range">Length of the line.</param>
    /// <returns>List of tiles in the line.</returns>
    public List<Tile> AreaLine(Tile center, Tile target, int range)
    {
        return RangeCalculator.AreaLine(_GameBoard.PlayArea, center, target, range);
    }

    /// <summary>
    /// Wrapper for <see cref="RangeCalculator.AreaCone"/>.
    /// </summary>
    /// <param name="center">Starting tile.</param>
    /// <param name="range">Depth of the cone.</param>
    /// <param name="direction">Index indicating the cone direction.</param>
    /// <returns>Tiles contained within the cone.</returns>
    public List<Tile> AreaCone(Tile center, Tile target, int range)
    {
        return RangeCalculator.AreaCone(_GameBoard.PlayArea, center, target, range);
    }
}
