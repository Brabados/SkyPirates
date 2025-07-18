using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIMoveable
{
    public List<int> MoveToHeatMap(List<Tile> TilesInRange, List<Tile> PlayerPos, List<Tile> FriendlyPos, int movement);

}
