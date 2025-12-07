using Unity.Entities;
using Unity.Transforms;


[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct FlockCenterUpdateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, settings, data)
                 in SystemAPI.Query<RefRO<LocalTransform>, RefRO<FlockCenterUpdateSettings>, RefRW<FlockCenterData>>())
        {
            if (settings.ValueRO.UpdatePosition)
            {
                data.ValueRW.Position = transform.ValueRO.Position;
            }
        }
    }
}
public struct FlockCenterUpdateSettings : IComponentData
{
    public bool UpdatePosition;
}
