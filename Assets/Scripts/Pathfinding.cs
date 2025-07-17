using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    private const int MOVE_COST = 10;

    public List<Vector3Int> FindPath(Tile startTile, Tile endTile, Tile[] allTiles)
    {
        Dictionary<Tile, int> tileIndexMap = BuildTileIndexMap(allTiles);
        List<PathNode> pathNodes = BuildPathNodes(allTiles, endTile, tileIndexMap);
        List<int> pathIndices = new List<int>();

        if (FindPathInternal(startTile, endTile, pathNodes, pathIndices, tileIndexMap))
        {
            return ConvertIndicesToPositions(pathIndices, allTiles);
        }

        return new List<Vector3Int>();
    }

    private Dictionary<Tile, int> BuildTileIndexMap(Tile[] allTiles)
    {
        Dictionary<Tile, int> tileIndexMap = new Dictionary<Tile, int>();
        for (int i = 0; i < allTiles.Length; i++)
        {
            tileIndexMap[allTiles[i]] = i;
        }
        return tileIndexMap;
    }

    private List<PathNode> BuildPathNodes(Tile[] allTiles, Tile endTile, Dictionary<Tile, int> tileIndexMap)
    {
        List<PathNode> pathNodes = new List<PathNode>(allTiles.Length);
        foreach (Tile tile in allTiles)
        {
            pathNodes.Add(new PathNode
            {
                Tile = tile,
                GCost = int.MaxValue,
                HCost = CalculateHexDistance(tile, endTile),
                IsWalkable = tile.Data.MovementCost > 0,
                CameFrom = null
            });
        }
        return pathNodes;
    }

    private bool FindPathInternal(Tile startTile, Tile endTile, List<PathNode> pathNodes, List<int> pathIndices, Dictionary<Tile, int> tileIndexMap)
    {
        int startNodeIndex = tileIndexMap[startTile];
        int endNodeIndex = tileIndexMap[endTile];

        PathNode startNode = pathNodes[startNodeIndex];
        startNode.GCost = 0;

        List<PathNode> openList = new List<PathNode> { startNode };
        HashSet<PathNode> closedList = new HashSet<PathNode>();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestCostFNode(openList);
            if (currentNode.Tile == endTile)
            {
                break;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Tile neighbor in currentNode.Tile.Neighbours)
            {
                if (neighbor == null || !tileIndexMap.ContainsKey(neighbor)) continue;

                PathNode neighborNode = pathNodes[tileIndexMap[neighbor]];
                if (closedList.Contains(neighborNode) || !neighborNode.IsWalkable) continue;

                int tentativeGCost = currentNode.GCost + MOVE_COST;
                if (tentativeGCost < neighborNode.GCost)
                {
                    neighborNode.CameFrom = currentNode;
                    neighborNode.GCost = tentativeGCost;
                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }

        PathNode endNode = pathNodes[endNodeIndex];
        if (endNode.CameFrom != null || endNode == startNode)
        {
            CalculatePathIndices(startNode, endNode, pathIndices, tileIndexMap);
            pathIndices.Reverse(); // Reverse the path to start from the start node
            return true;
        }
        return false;
    }

    private void CalculatePathIndices(PathNode startNode, PathNode endNode, List<int> pathIndices, Dictionary<Tile, int> tileIndexMap)
    {
        PathNode currentNode = endNode;
        while (currentNode != startNode)
        {
            pathIndices.Add(tileIndexMap[currentNode.Tile]);
            currentNode = currentNode.CameFrom;
        }
        pathIndices.Add(tileIndexMap[startNode.Tile]); // Add the start node to the path
    }

    private int CalculateHexDistance(Tile a, Tile b)
    {
        return (Mathf.Abs(a.QAxis - b.QAxis) + Mathf.Abs(a.RAxis - b.RAxis) + Mathf.Abs(a.SAxis - b.SAxis)) / 2;
    }

    private PathNode GetLowestCostFNode(List<PathNode> openList)
    {
        PathNode lowestCostNode = openList[0];
        foreach (var node in openList)
        {
            if (node.FCost < lowestCostNode.FCost)
            {
                lowestCostNode = node;
            }
        }
        return lowestCostNode;
    }

    private List<Vector3Int> ConvertIndicesToPositions(List<int> pathIndices, Tile[] allTiles)
    {
        List<Vector3Int> finalPath = new List<Vector3Int>(pathIndices.Count);
        foreach (int index in pathIndices)
        {
            Tile tile = allTiles[index];
            finalPath.Add(new Vector3Int(tile.QAxis, tile.RAxis, tile.SAxis));
        }
        return finalPath;
    }

    private class PathNode
    {
        public Tile Tile;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;
        public bool IsWalkable;
        public PathNode CameFrom;
    }
}
public struct PathfinderSelections
{
    public List<List<Vector3Int>> Paths; // List of paths, where each path is a list of Vector3Ints
    public int NumSelections; // Number of selections or paths stored

    public PathfinderSelections(List<List<Vector3Int>> paths, int numSelections)
    {
        Paths = paths;
        NumSelections = numSelections;
    }

    public void AddPath(List<Vector3Int> path)
    {
        if (Paths == null)
            Paths = new List<List<Vector3Int>>();

        Paths.Add(path);
        NumSelections = Paths.Count;
    }

    public void ClearPaths()
    {
        if (Paths != null)
            Paths.Clear();

        NumSelections = 0;
    }

    public void ClearLastPath()
    {
        if (Paths.Count != 0)
        {
            Paths.RemoveAt(Paths.Count - 1);
            NumSelections = Paths.Count;
        }
    }
    public int TotalPathLength()
    {
        if (Paths == null || Paths.Count == 0) return 0;

        int total = 0;
        foreach (var path in Paths)
        {
            total += path.Count;
        }

        // Subtract 1 for each path (each includes its start tile)
        return total - Paths.Count;
    }
}
