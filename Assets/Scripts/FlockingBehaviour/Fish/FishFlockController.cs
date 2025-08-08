using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct FishPrefabComponent : IComponentData
{
    public Entity prefab;
}

public partial class FishFlockController : SystemBase
{
    private bool _hasSpawned;

    protected override void OnCreate()
    {
        RequireForUpdate<FishPrefabComponent>();
    }

    protected override void OnStartRunning()
    {
        if (_hasSpawned) return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var prefabEntity = SystemAPI.GetSingleton<FishPrefabComponent>().prefab;

        // Get settings from component, or use defaults if not found
        FishControllerSettingsComponent settings = default;
        bool hasSettings = SystemAPI.HasSingleton<FishControllerSettingsComponent>();

        if (hasSettings)
        {
            settings = SystemAPI.GetSingleton<FishControllerSettingsComponent>();
            Debug.Log("Using FishControllerSettings from component");
        }
        else
        {
            // Default values if no settings component found
            settings = new FishControllerSettingsComponent
            {
                SpawnCount = 1000,
                SpawnRadius = 10f,
                InitialSpeed = 2f,
                MaxSpeed = 5f,
                SearchRadius = 5f,
                CohesionWeight = 1f,
                AlignmentWeight = 1f,
                SeparationWeight = 1f,
                ObstacleAvoidanceDistance = 2f,
                ObstacleAvoidanceWeight = 1f,
                BoundaryCenter = float3.zero,
                BoundaryRadius = 20f,
                BoundaryWeight = 2f
            };
            Debug.Log("Using default FishControllerSettings (no settings component found)");
        }

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

        for (int i = 0; i < settings.SpawnCount; i++)
        {
            var instance = entityManager.Instantiate(prefabEntity);

            float3 pos = random.NextFloat3Direction() * random.NextFloat(0f, settings.SpawnRadius);
            float3 forward = random.NextFloat3Direction();

            entityManager.SetComponentData(instance, new LocalTransform
            {
                Position = pos,
                Rotation = quaternion.LookRotationSafe(forward, math.up()),
                Scale = 1f
            });

            entityManager.SetComponentData(instance, new Velocity
            {
                Value = forward * settings.InitialSpeed
            });

            entityManager.SetComponentData(instance, new BoidSettings
            {
                MaxSpeed = settings.MaxSpeed,
                SearchRadius = settings.SearchRadius,
                ObstacleAvoidanceDistance = settings.ObstacleAvoidanceDistance,
                ObstacleAvoidanceWeight = settings.ObstacleAvoidanceWeight,
                CohesionWeight = settings.CohesionWeight,
                AlignmentWeight = settings.AlignmentWeight,
                SeparationWeight = settings.SeparationWeight
            });

            entityManager.SetComponentData(instance, new BoundarySettings
            {
                Center = settings.BoundaryCenter,
                Radius = settings.BoundaryRadius,
                BoundaryWeight = settings.BoundaryWeight
            });

            entityManager.AddComponent<BoidTag>(instance);
        }

        _hasSpawned = true;
        Debug.Log($"Spawned {settings.SpawnCount} fish with FishFlockController using " + (hasSettings ? "custom" : "default") + " settings.");
    }

    protected override void OnUpdate() { }
}
