using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnState : HexSelectState
{
    private BasicMovement movementLogic;
    private RangeFinder rangeFinder;
    private Board board;
    private Pathfinding pathfinder = new Pathfinding();

    public override void EnterState(HexSelectManager manager)
    {
        Pawn enemy = TurnManager.Instance.currentTurn.Owner;
        Map map = GameObject.FindObjectOfType<Map>();
        board = map.PlayArea;
        if (enemy == null || !(enemy is EnemyPawn)) return;

        movementLogic = enemy.GetComponent<BasicMovement>();
        movementLogic.board = board;
        if (movementLogic == null || movementLogic.board == null)
        {
            Debug.LogError("EnemyMoveState: Missing BasicMovement or Board.");
            return;
        }

        rangeFinder = manager.HighlightFinder;
        board = movementLogic.board;

        Tile startTile = enemy.Position;
        int moveLeft = TurnManager.Instance.currentTurn.MovementLeft;

        List<Tile> reachable = rangeFinder.HexReachable(startTile, moveLeft);
        if (reachable == null || reachable.Count == 0)
        {
            Debug.Log("EnemyMoveState: No reachable tiles.");
            manager.SwitchToDefaultState();
            return;
        }

        // Gather player and friendly tiles
        List<Tile> playerTiles = new List<Tile>();
        foreach (Pawn p in PawnManager.PawnManagerInstance.PlayerPawns)
            playerTiles.Add(p.Position);

        List<Tile> friendlies = new List<Tile>();
        foreach (Pawn e in PawnManager.PawnManagerInstance.EnemyPawns)
            if (e != enemy)
                friendlies.Add(e.Position);

        // Get scores
        List<int> heatmap = movementLogic.MoveToHeatMap(reachable, playerTiles, friendlies, moveLeft);

        // Score & select top 5
        List<(Tile tile, int score)> scored = new List<(Tile tile, int score)>();
        for (int i = 0; i < reachable.Count; i++)
            scored.Add((reachable[i], heatmap[i]));

        scored.Sort((a, b) => b.score.CompareTo(a.score)); // high to low
        int topN = Mathf.Min(5, scored.Count);

        Tile target = scored[Random.Range(0, topN)].tile;

        // Build array for pathfinding
        Tile[] legalTiles = reachable.ToArray();

        // Find path
        List<Vector3Int> pathCubes = pathfinder.FindPath(startTile, target, legalTiles);
        if (pathCubes == null || pathCubes.Count < 2)
        {
            Debug.Log("EnemyMoveState: No valid path.");
            manager.SwitchToDefaultState();
            return;
        }

        // Move enemy pawn
        Vector3Int lastCube = pathCubes[pathCubes.Count - 1];
        Tile destination = board.SearchTileByCubeCoordinates(lastCube.x, lastCube.y, lastCube.z);
        if (destination != null)
        {
            enemy.SetPosition(destination);
            TurnManager.Instance.currentTurn.MovementLeft -= (pathCubes.Count - 1);
        }

        // Done, return to default
        manager.SwitchToDefaultState();
    }

    public override void UpdateState(HexSelectManager manager) { }
    public override void ExitState(HexSelectManager manager) { }
}
