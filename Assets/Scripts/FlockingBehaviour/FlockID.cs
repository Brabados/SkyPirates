using Unity.Entities;
using Unity.Mathematics;

public struct FishPrefabComponent : IComponentData
{
    public Entity prefab;
}

public struct BoidFlockID : IComponentData
{
    public int FlockID;
}

public struct FlockCenterData : IComponentData
{
    public int FlockID;
    public float3 Position;
}

// Buffer element for flock prefab selection
public struct FlockPrefabSelection : IBufferElementData
{
    public int PrefabIndex; // -1 means use all prefabs
    public Entity PrefabEntity; // Direct entity reference (if set, overrides PrefabIndex)
}
