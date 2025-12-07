using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct FishPrefabInitializerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // Run only once when prefabs are available
        state.RequireForUpdate<FishPrefabReference>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // If singleton already exists, do nothing
        if (SystemAPI.HasSingleton<FishPrefabComponent>())
            return;

        // Find first prefab from FishPrefabReference buffer
        Entity prefabEntity = Entity.Null;
        foreach (var (buffer, entity) in SystemAPI.Query<DynamicBuffer<FishPrefabReference>>().WithEntityAccess())
        {
            if (buffer.Length > 0)
            {
                prefabEntity = buffer[0].Prefab;
                break;
            }
        }

        if (prefabEntity == Entity.Null)
            return; // No prefab found yet

        // Create singleton entity with FishPrefabComponent
        Entity singletonEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(singletonEntity, new FishPrefabComponent
        {
            prefab = prefabEntity
        });

#if UNITY_EDITOR
        state.EntityManager.SetName(singletonEntity, "FishPrefabSingleton");
#endif
    }
}
