using System.Collections.Generic;

public interface IAIHeatMap
{
    public List<int> HeatMap(List<Tile> TilesInRange, List<Tile> PlayerPos, List<Tile> FriendlyPos, int movement);

}
