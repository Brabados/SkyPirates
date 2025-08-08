using Unity.Entities;
using Unity.Mathematics;

public struct BoundarySettings : IComponentData
{
    public float3 Center;
    public float Radius;
    public float BoundaryWeight;
}
