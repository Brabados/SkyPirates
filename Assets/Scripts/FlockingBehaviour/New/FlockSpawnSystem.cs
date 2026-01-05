using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct FlockSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FlockCenterData>();
        state.RequireForUpdate<BoidSettings>();
        state.RequireForUpdate<FishPrefabReference>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Global boid behaviour (safe: OnUpdate only)
        BoidSettings boidSettings = SystemAPI.GetSingleton<BoidSettings>();

        // Find prefab registry
        var registryQuery = SystemAPI.QueryBuilder()
            .WithAll<FishPrefabReference>()
            .Build();

        if (registryQuery.CalculateEntityCount() != 1)
        {
            UnityEngine.Debug.LogError(
                $"Expected exactly 1 FishPrefabRegistry, found {registryQuery.CalculateEntityCount()}");
            return;
        }

        Entity registryEntity = registryQuery.GetSingletonEntity();
        DynamicBuffer<FishPrefabReference> registryPrefabs =
            state.EntityManager.GetBuffer<FishPrefabReference>(registryEntity);

        if (registryPrefabs.Length == 0)
        {
            UnityEngine.Debug.LogError("FishPrefabRegistry contains zero prefabs");
            return;
        }

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (center, spawnSettings, flockPrefabs, entity) in
                 SystemAPI.Query<
                     RefRO<FlockCenterData>,
                     RefRO<FlockSpawnSettings>,
                     DynamicBuffer<FlockPrefabSelection>
                 >()
                 .WithNone<SpawnFlockOnce>()
                 .WithEntityAccess())
        {
            SpawnFlock(
                center.ValueRO,
                spawnSettings.ValueRO,
                boidSettings,
                flockPrefabs,
                registryPrefabs,
                ecb
            );

            ecb.AddComponent<SpawnFlockOnce>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    // -------------------------
    // Static helpers (PURE)
    // -------------------------

    private static void SpawnFlock(
        FlockCenterData center,
        FlockSpawnSettings spawnSettings,
        BoidSettings boidSettings,
        DynamicBuffer<FlockPrefabSelection> flockPrefabs,
        DynamicBuffer<FishPrefabReference> registryPrefabs,
        EntityCommandBuffer ecb)
    {
        uint seed = (uint)(center.FlockID * 73856093);
        var random = new Unity.Mathematics.Random(seed == 0 ? 1u : seed);

        for (int i = 0; i < spawnSettings.SpawnCount; i++)
        {
            Entity prefab = SelectPrefab(
                ref random,
                center.FlockID,
                flockPrefabs,
                registryPrefabs
            );

            Entity boid = ecb.Instantiate(prefab);

            float3 position =
                center.Position +
                RandomInsideSphere(ref random, spawnSettings.SpawnRadius);

            float3 velocity =
                RandomDirection(ref random) * spawnSettings.InitialSpeed;

            ecb.AddComponent(boid, LocalTransform.FromPosition(position));
            ecb.AddComponent(boid, new Velocity { Value = velocity });
            ecb.AddComponent(boid, new BoidFlockID { FlockID = center.FlockID });

            // Global movement behaviour
            ecb.AddComponent(boid, boidSettings);
        }
    }

    private static Entity SelectPrefab(
        ref Unity.Mathematics.Random random,
        int flockID,
        DynamicBuffer<FlockPrefabSelection> flockPrefabs,
        DynamicBuffer<FishPrefabReference> registryPrefabs)
    {
        // Explicit prefab entities
        if (flockPrefabs.Length > 0 && flockPrefabs[0].PrefabEntity != Entity.Null)
        {
            int index = random.NextInt(flockPrefabs.Length);
            return flockPrefabs[index].PrefabEntity;
        }

        // Indexed selection into registry
        if (flockPrefabs.Length > 0 && flockPrefabs[0].PrefabIndex >= 0)
        {
            int index = random.NextInt(flockPrefabs.Length);
            int registryIndex =
                math.abs(flockPrefabs[index].PrefabIndex) % registryPrefabs.Length;

            return registryPrefabs[registryIndex].Prefab;
        }

        // Fallback: deterministic by flock ID
        int fallback = math.abs(flockID) % registryPrefabs.Length;
        return registryPrefabs[fallback].Prefab;
    }

    private static float3 RandomDirection(ref Unity.Mathematics.Random random)
    {
        float3 v;
        do
        {
            v = random.NextFloat3(-1f, 1f);
        }
        while (math.lengthsq(v) < 0.0001f);

        return math.normalize(v);
    }

    private static float3 RandomInsideSphere(
        ref Unity.Mathematics.Random random,
        float radius)
    {
        return RandomDirection(ref random) * random.NextFloat(0f, radius);
    }
}
