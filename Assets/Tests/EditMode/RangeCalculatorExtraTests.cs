using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RangeCalculatorExtraTests
{
    // Helper copied from RangeCalculatorTests
    private Board CreateSimpleBoard(int size)
    {
        Board board = new Board(new Vector2Int(size, size));
        Map map = new GameObject("TestMap").AddComponent<Map>();
        map.isFlatTopped = false;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject go = new GameObject($"Tile_{x}_{y}");
                Tile tile = go.AddComponent<Tile>();
                tile.Data = ScriptableObject.CreateInstance<TileDataSO>();
                tile.Data.MovementCost = 1;
                tile.SetPositionAndHeight(new Vector2Int(x, y), x - size / 2, y - size / 2, 0);
                board.set_Tile(x, y, tile);
            }
        }

        map.SetNeighbours(board, map.isFlatTopped);
        return board;
    }

    [Test]
    public void HexScale_ScalesCoordinatesCorrectly()
    {
        Board board = CreateSimpleBoard(5);
        Tile start = board.get_Tile(3, 2); // q=1,r=0
        Tile expected = board.get_Tile(4, 2); // q=2,r=0
        Tile result = RangeCalculator.HexScale(board, start, 2);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void HexScale_WithZeroFactor_ReturnsCenter()
    {
        Board board = CreateSimpleBoard(3);
        Tile start = board.get_Tile(2, 1); // q=1,r=0
        Tile expected = board.get_Tile(1, 1); // origin
        Tile result = RangeCalculator.HexScale(board, start, 0);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void HexScale_NegativeFactor_ReflectsAcrossOrigin()
    {
        Board board = CreateSimpleBoard(5);
        Tile start = board.get_Tile(3, 2); // q=1,r=0
        Tile expected = board.get_Tile(1, 2); // q=-1,r=0
        Tile result = RangeCalculator.HexScale(board, start, -1);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void HexScale_OutOfBounds_ReturnsNull()
    {
        Board board = CreateSimpleBoard(5);
        Tile start = board.get_Tile(3, 2); // q=1,r=0
        Tile result = RangeCalculator.HexScale(board, start, 3);
        Assert.IsNull(result);
    }

    [Test]
    public void AreaRing_RadiusZero_ReturnsOnlyCenter()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        List<Tile> area = RangeCalculator.AreaRing(board, center, 0);
        Assert.AreEqual(1, area.Count);
        Assert.Contains(center, area);
    }

    [Test]
    public void AreaRing_RadiusOne_OnThreeByThreeBoard_ReturnsSevenTiles()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        List<Tile> area = RangeCalculator.AreaRing(board, center, 1);
        Assert.AreEqual(7, area.Count);
    }

    [Test]
    public void AreaRing_LargeRadius_TruncatedByBoardSize()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        List<Tile> area = RangeCalculator.AreaRing(board, center, 3);
        Assert.AreEqual(board._size_X * board._size_Y, area.Count);
    }

    [Test]
    public void AreaLine_CreatesStraightLine()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        Tile target = center.Neighbours[0];
        List<Tile> line = RangeCalculator.AreaLine(board, center, target, 1);
        Assert.AreEqual(1, line.Count);
        Assert.AreEqual(target, line[0]);
    }

    [Test]
    public void AreaLine_StopsAtEdge()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        Tile target = center.Neighbours[0];
        List<Tile> line = RangeCalculator.AreaLine(board, center, target, 2);
        Assert.AreEqual(1, line.Count);
    }

    [Test]
    public void AreaLine_NoDirection_ReturnsEmpty()
    {
        Board board = CreateSimpleBoard(3);
        Tile center = board.get_Tile(1, 1);
        List<Tile> line = RangeCalculator.AreaLine(board, center, center, 3);
        Assert.AreEqual(0, line.Count);
    }

}

