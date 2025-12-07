using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight EditMode tests for the <see cref="PawnManager"/> helper
/// functions.  Rather than loading prefabs, each test constructs a simple
/// <see cref="PawnManager"/> populated with mock <see cref="PlayerPawns"/> and
/// <see cref="EnemyPawn"/> components so that the lookup logic can be verified
/// in isolation.
/// </summary>
public class PawnManagerTests
{
    /// <summary>
    /// Utility used by the tests to create a temporary manager instance with a
    /// set number of player and enemy pawns.
    /// </summary>
    private PawnManager CreateManager(int playerCount, int enemyCount)
    {
        var go = new GameObject("PawnManager");
        var manager = go.AddComponent<PawnManager>();
        manager.PlayerPawns = new List<PlayerPawns>();
        manager.EnemyPawns = new List<EnemyPawn>();

        for (int i = 0; i < playerCount; i++)
        {
            var playerObj = new GameObject($"Player{i}");
            var player = playerObj.AddComponent<PlayerPawns>();
            manager.PlayerPawns.Add(player);
        }

        for (int i = 0; i < enemyCount; i++)
        {
            var enemyObj = new GameObject($"Enemy{i}");
            var enemy = enemyObj.AddComponent<EnemyPawn>();
            manager.EnemyPawns.Add(enemy);
        }

        return manager;
    }

    // getPlayerPawnPosition should return the index of the matching player
    // object or -1 when not found. This test ensures a known pawn is resolved
    // to its expected index.
    [Test]
    public void GetPlayerPawnPosition_ReturnsExpectedIndex()
    {
        var manager = CreateManager(3, 0);
        int index = manager.getPlayerPawnPosition(manager.PlayerPawns[1].gameObject);
        Assert.AreEqual(1, index);
    }

    // Similar to the player lookup, validate that getEnemyPawnPosition returns
    // the correct list index for an enemy pawn component.
    [Test]
    public void GetEnemyPawnPosition_ReturnsExpectedIndex()
    {
        var manager = CreateManager(0, 3);
        int index = manager.getEnemyPawnPosition(manager.EnemyPawns[2]);
        Assert.AreEqual(2, index);
    }

    // GetNextPlayerPawnByObject should return the index immediately after the
    // provided object, wrapping to zero if the object is the last entry.
    [Test]
    public void GetNextPlayerPawnByObject_ReturnsNextIndex()
    {
        var manager = CreateManager(3, 0);
        // When not last element, GetNextPlayerPawnByObject should return the
        // index following the object passed in.
        int next = manager.GetNextPlayerPawnByObject(manager.PlayerPawns[0].gameObject);
        Assert.AreEqual(1, next);

        // When the last element is passed in the call should wrap back to
        // the first index (0).
        int wrap = manager.GetNextPlayerPawnByObject(manager.PlayerPawns[2].gameObject);
        Assert.AreEqual(0, wrap);
    }
}
