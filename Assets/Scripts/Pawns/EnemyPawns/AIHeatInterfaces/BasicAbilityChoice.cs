using System.Collections.Generic;
using UnityEngine;


public class BasicAbilityChoice : MonoBehaviour, IAIHeatMap
{
    public List<int> HeatMap(List<Tile> TilesInRange, List<Tile> PlayerPos, List<Tile> FriendlyPos, int movement)
    {
        List<int> heatmap = new List<int>(TilesInRange.Count);

        Pawn enemy = GetComponent<Pawn>();
        if (enemy?.Equiped?.Weapon?.BaseAttack == null)
        {
            for (int i = 0; i < TilesInRange.Count; i++) heatmap.Add(0);
            return heatmap;
        }

        ActiveAbility ability = enemy?.Equiped?.Weapon?.BaseAttack;
        RangeFinder finder = HexSelectManager.Instance.HighlightFinder;

        foreach (Tile origin in TilesInRange)
        {
            int totalScore = 0;

            foreach (BaseAction action in ability.Actions)
            {
                int bestScoreForAction = 0;

                // Select relevant targets based on action's Targettype
                IEnumerable<Tile> targets = GetRelevantTargets(action.Targettype, PlayerPos, FriendlyPos, TilesInRange, origin, enemy);

                foreach (Tile target in targets)
                {
                    if (!IsValidTarget(action.Targettype, target, PlayerPos, FriendlyPos, origin, enemy))
                        continue;

                    List<Tile> area = GetTilesForAction(action, origin, target, finder);
                    int hits = CountHits(action.Targettype, area, PlayerPos, FriendlyPos, enemy);

                    if (hits == 0) continue; // Skip this target if it hits nothing
                    if (hits > bestScoreForAction)
                        bestScoreForAction = hits;
                }

                totalScore += bestScoreForAction * 10;
            }

            int closest = ClosestDistance(origin, PlayerPos);
            int distanceScore = (closest > 0) ? 5 - closest : 0;

            heatmap.Add(totalScore + distanceScore);
        }

        return heatmap;
    }

    private IEnumerable<Tile> GetRelevantTargets(Target type, List<Tile> players, List<Tile> friends, List<Tile> range, Tile origin, Pawn self)
    {
        if (type == Target.Enemy)
        {
            return players;
        }
        else if (type == Target.Friendly)
        {
            return friends;
        }
        else if (type == Target.Self)
        {
            return new List<Tile> { origin };
        }
        else if (type == Target.Pawn)
        {
            List<Tile> combined = new List<Tile>(players);
            combined.AddRange(friends);
            return combined;
        }
        else
        {
            return range; // For Target.Tile or unknown values
        }
    }


    private bool IsValidTarget(Target type, Tile t, List<Tile> players, List<Tile> friends, Tile origin, Pawn self)
    {
        return type switch
        {
            Target.Enemy => players.Contains(t),
            Target.Friendly => friends.Contains(t),
            Target.Pawn => players.Contains(t) || friends.Contains(t),
            Target.Self => t == origin,
            Target.Tile => true,
            _ => false,
        };
    }

    private int CountHits(Target type, List<Tile> area, List<Tile> players, List<Tile> friends, Pawn self)
    {
        int count = 0;
        foreach (Tile tile in area)
        {
            switch (type)
            {
                case Target.Enemy: if (players.Contains(tile)) count++; break;
                case Target.Friendly: if (friends.Contains(tile)) count++; break;
                case Target.Pawn: if (players.Contains(tile) || friends.Contains(tile)) count++; break;
                case Target.Tile: count++; break;
                case Target.Self: if (tile == self.Position) count++; break;
            }
        }
        return count;
    }

    private List<Tile> GetTilesForAction(BaseAction action, Tile origin, Tile target, RangeFinder finder)
    {
        int usedRange = action.MaxRange ? action.Range : HexUtils.CubeDistance(origin, target);

        return action.Area switch
        {
            EffectArea.Single => new List<Tile> { target },
            EffectArea.Area => finder.AreaRing(target, action.Size),
            EffectArea.Ring => finder.HexRing(target, action.Size),
            EffectArea.Line => finder.AreaLine(origin, target, usedRange),
            EffectArea.Cone => finder.AreaCone(origin, target, usedRange, action.Size),
            EffectArea.Diagonal => finder.AreaDiagonal(target, usedRange),
            EffectArea.Path => finder.AreaPath(origin, target, usedRange),
            _ => new List<Tile>(),
        };
    }

    private int ClosestDistance(Tile origin, List<Tile> targets)
    {
        int min = int.MaxValue;
        foreach (Tile t in targets)
        {
            int d = HexUtils.CubeDistance(origin, t);
            if (d < min) min = d;
        }
        return min == int.MaxValue ? 0 : min;
    }
}
