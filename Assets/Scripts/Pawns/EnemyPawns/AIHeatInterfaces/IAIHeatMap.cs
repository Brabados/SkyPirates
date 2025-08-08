using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIHeatMap
{
    public List<int> HeatMap(List<Tile> TilesInRange, List<Tile> PlayerPos, List<Tile> FriendlyPos, int movement);

}
