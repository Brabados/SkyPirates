using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour, IAIHeatMap
{
    public Board board; // Assumes Board is assigned externally

    public List<int> HeatMap(List<Tile> TilesInRange, List<Tile> PlayerPos, List<Tile> FriendlyPos, int movement)
    {
        List<int> heatMap = new List<int>();

        foreach (Tile tile in TilesInRange)
        {
            int score = 0;

            // 1. Closer to Enemy (positive for closer, negative for further away)
            int minDistToEnemy = int.MaxValue;
            foreach (Tile enemy in PlayerPos)
            {
                int dist = board.CubeDistance(tile, enemy);
                if (dist < minDistToEnemy)
                    minDistToEnemy = dist;
            }
            // Assume enemy distance affects score inversely (closer = higher score)
            score += Mathf.Max(0, 10 - minDistToEnemy); // You can tweak "10" as a max weight

            // 2. Ally proximity within 3 tiles (max at distance 3, less when closer, negative if > 3)
            int minDistToAlly = int.MaxValue;
            foreach (Tile ally in FriendlyPos)
            {
                int dist = board.CubeDistance(tile, ally);
                if (dist < minDistToAlly)
                    minDistToAlly = dist;
            }

            if (minDistToAlly <= 3)
            {
                // Best score at distance 3, worst (but still positive) at distance 1
                score += minDistToAlly; 
            }
            else
            {
                score -= (minDistToAlly - 3); // Penalty for being far from allies
            }

            // 3. Movement remaining bonus (penalize 0, give more value to remaining movement)
            if (movement <= 0)
            {
                score -= 5; // Penalty for having no movement left
            }
            else
            {
                score += Mathf.Clamp(movement, 1, 5); // Cap to prevent runaway values
            }

            heatMap.Add(score);
        }

        return heatMap;
    }
}
