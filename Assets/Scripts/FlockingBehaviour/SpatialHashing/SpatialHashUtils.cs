using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public static class SpatialHashUtils
{
    [BurstCompile]
    public static int3 GetSpatialHash(float3 position, float nodeSize)
    {
        return new int3(
            (int)math.floor(position.x / nodeSize),
            (int)math.floor(position.y / nodeSize),
            (int)math.floor(position.z / nodeSize)
        );
    }

    public static void GetNeighborOffsets(ref NativeArray<int3> offsets)
    {
        int index = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    offsets[index++] = new int3(x, y, z);
                }
            }
        }
    }

    public static NativeArray<int3> GetNeighborCellsUnburst(int3 centerCell, Allocator allocator)
    {
        var neighbors = new NativeArray<int3>(27, allocator);
        int index = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    neighbors[index++] = centerCell + new int3(x, y, z);
                }
            }
        }
        return neighbors;
    }

    public static float GetHashDistance(int3 hash1, int3 hash2, float cellSize)
    {
        int3 diff = hash1 - hash2;
        return math.length(new float3(diff.x, diff.y, diff.z)) * cellSize;
    }

    public static bool IsWithinRadius(int3 centerHash, int3 testHash, int radiusInCells)
    {
        int3 diff = math.abs(testHash - centerHash);
        return diff.x <= radiusInCells && diff.y <= radiusInCells && diff.z <= radiusInCells;
    }
}
