using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
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
}
