using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


// Enhanced FishFlockController with prefab selection and closest spawn
public partial class FishFlockController : SystemBase
{
    private bool _hasSpawned;
    private NativeHashSet<int> _spawnedFlocks;
    private static FishFlockController _instance;

    protected override void OnCreate()
    {
        RequireForUpdate<FishPrefabComponent>();
        _spawnedFlocks = new NativeHashSet<int>(10, Allocator.Persistent);
        _instance = this;

        // Clean up duplicate settings on startup
        CleanupDuplicateSettings();
    }

    private void CleanupDuplicateSettings()
    {
        var settingsQuery = EntityManager.CreateEntityQuery(typeof(FishControllerSettingsComponent));
        int count = settingsQuery.CalculateEntityCount();

        if (count > 1)
        {
            Debug.LogWarning($"Found {count} FishControllerSettingsComponent on startup, removing duplicates");
            var entities = settingsQuery.ToEntityArray(Allocator.Temp);

            for (int i = 1; i < entities.Length; i++)
            {
                EntityManager.DestroyEntity(entities[i]);
            }

            entities.Dispose();
        }

        settingsQuery.Dispose();
    }

    protected override void OnDestroy()
    {
        if (_spawnedFlocks.IsCreated)
            _spawnedFlocks.Dispose();
        if (_instance == this)
            _instance = null;
    }

    protected override void OnStartRunning()
    {
        if (_hasSpawned) return;
        SpawnForAllFlockCenters();
        _hasSpawned = true;
    }

    /// <summary>
    /// Public method to spawn at the closest flock center to a given position
    /// </summary>
    public void TriggerSpawnAtClosest(float3 position)
    {
        int closestFlockId = FindClosestFlockCenter(position);
        if (closestFlockId >= 0)
        {
            Debug.Log($"Spawning additional boids at closest flock center (FlockID: {closestFlockId})");
            SpawnFish(closestFlockId);
        }
        else
        {
            Debug.LogWarning("No flock centers found for closest spawn");
        }
    }

    /// <summary>
    /// Static accessor for external scripts
    /// </summary>
    public static FishFlockController Instance => _instance;

    private int FindClosestFlockCenter(float3 position)
    {
        var query = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        if (query.IsEmpty) return -1;

        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
        var flockCenters = query.ToComponentDataArray<FlockCenterData>(Allocator.Temp);

        float closestDistanceSq = float.MaxValue;
        int closestFlockId = -1;

        for (int i = 0; i < flockCenters.Length; i++)
        {
            float distanceSq = math.lengthsq(position - flockCenters[i].Position);
            if (distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestFlockId = flockIDs[i].FlockID;
            }
        }

        flockIDs.Dispose();
        flockCenters.Dispose();
        return closestFlockId;
    }

    private void SpawnForAllFlockCenters()
    {
        var query = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        if (query.IsEmpty)
        {
            Debug.LogWarning("No flock centers found - spawning default flock at origin");
            SpawnFish(0);
            return;
        }

        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
        var flockCenters = query.ToComponentDataArray<FlockCenterData>(Allocator.Temp);

        for (int i = 0; i < flockIDs.Length; i++)
        {
            int flockId = flockIDs[i].FlockID;
            if (!_spawnedFlocks.Contains(flockId))
            {
                SpawnFish(flockId);
                _spawnedFlocks.Add(flockId);
                Debug.Log($"Auto-spawned boids for FlockID {flockId} at {flockCenters[i].Position}");
            }
        }

        flockIDs.Dispose();
        flockCenters.Dispose();
    }

    public void TriggerSpawn(int flockId)
    {
        SpawnFish(flockId);
    }

    private void SpawnFish(int flockId)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get available prefabs for this flock
        var availablePrefabs = GetPrefabsForFlock(flockId);
        if (availablePrefabs.Length == 0)
        {
            Debug.LogError($"No prefabs available for FlockID {flockId}");
            availablePrefabs.Dispose();
            return;
        }

        // Get settings - MANUAL QUERY to avoid singleton validation
        FishControllerSettingsComponent settings;
        var settingsQuery = EntityManager.CreateEntityQuery(typeof(FishControllerSettingsComponent));
        int settingsCount = settingsQuery.CalculateEntityCount();

        if (settingsCount > 1)
        {
            Debug.LogWarning($"Found {settingsCount} FishControllerSettingsComponent instances, cleaning up duplicates");
            var entities = settingsQuery.ToEntityArray(Allocator.Temp);

            // Keep the first one, destroy the rest
            for (int i = 1; i < entities.Length; i++)
            {
                EntityManager.DestroyEntity(entities[i]);
            }

            entities.Dispose();
            settingsCount = 1;
        }

        if (settingsCount > 0)
        {
            var settingsArray = settingsQuery.ToComponentDataArray<FishControllerSettingsComponent>(Allocator.Temp);
            settings = settingsArray[0];
            settingsArray.Dispose();
        }
        else
        {
            settings = GetDefaultSettings();
        }

        settingsQuery.Dispose();

        // Get spawn position
        float3 spawnCenter = GetFlockCenterPosition(flockId, settings.BoundaryCenter);

        var random = new Unity.Mathematics.Random((uint)(UnityEngine.Random.Range(1, int.MaxValue) + flockId * 1000));


        for (int i = 0; i < settings.SpawnCount; i++)
        {
            int prefabIndex = random.NextInt(0, availablePrefabs.Length);
            Entity selectedPrefab = availablePrefabs[prefabIndex];

            Entity instance = entityManager.Instantiate(selectedPrefab);

            // Ensure entity doesn’t get destroyed with the scene
            if (entityManager.HasComponent<SceneTag>(instance))
                entityManager.RemoveComponent<SceneTag>(instance);

            if (entityManager.HasBuffer<LinkedEntityGroup>(instance))
            {
                var buffer = entityManager.GetBuffer<LinkedEntityGroup>(instance);
                for (int j = 0; j < buffer.Length; j++)
                {
                    var child = buffer[j].Value;
                    if (entityManager.HasComponent<SceneTag>(child))
                        entityManager.RemoveComponent<SceneTag>(child);
                }
            }

            // Position + orientation
            float3 randomOffset = random.NextFloat3Direction() * random.NextFloat(0f, settings.SpawnRadius);
            float3 pos = spawnCenter + randomOffset;
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
                Center = spawnCenter,
                Radius = settings.BoundaryRadius,
                BoundaryWeight = settings.BoundaryWeight
            });

            entityManager.AddComponent<BoidTag>(instance);
            entityManager.AddComponentData(instance, new BoidFlockID { FlockID = flockId });
        }

        Debug.Log($"Spawned {settings.SpawnCount} boids for FlockID {flockId} at {spawnCenter} using {availablePrefabs.Length} different prefabs");
        availablePrefabs.Dispose();
    }
    public static void HideBoidsAndPause()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        using var allBoids = em.CreateEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<BoidTag>() },
            Options = EntityQueryOptions.IncludeDisabledEntities
        });

        em.AddComponent<Disabled>(allBoids);
    }

    /// <summary>
    /// Show + resume all boids (removes 'Disabled' from boids that have it).
    /// </summary>
    public static void ShowBoidsAndResume()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        using var pausedBoids = em.CreateEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<BoidTag>(),
                ComponentType.ReadOnly<Disabled>()
            },
            Options = EntityQueryOptions.IncludeDisabledEntities
        });

        em.RemoveComponent<Disabled>(pausedBoids);
    }


    private NativeArray<Entity> GetPrefabsForFlock(int flockId)
    {
        Debug.Log($"Getting prefabs for FlockID {flockId}");

        // Get all available prefabs from registry first
        var allPrefabs = GetAllAvailablePrefabs();

        // Try to find flock-specific prefab selection
        var flockCenterQuery = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        if (flockCenterQuery.IsEmpty)
        {
            Debug.Log($"No flock centers found, using all {allPrefabs.Length} prefabs for FlockID {flockId}");
            return allPrefabs;
        }

        var flockEntities = flockCenterQuery.ToEntityArray(Allocator.Temp);
        var flockIDs = flockCenterQuery.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

        Entity flockCenterEntity = Entity.Null;
        for (int i = 0; i < flockIDs.Length; i++)
        {
            if (flockIDs[i].FlockID == flockId)
            {
                flockCenterEntity = flockEntities[i];
                Debug.Log($"Found flock center entity for FlockID {flockId}: {flockCenterEntity}");
                break;
            }
        }

        flockEntities.Dispose();
        flockIDs.Dispose();

        // If no specific flock center found, use all prefabs
        if (flockCenterEntity == Entity.Null)
        {
            Debug.Log($"No flock center entity found for FlockID {flockId}, using all prefabs");
            return allPrefabs;
        }

        // Check if this flock center has prefab selection buffer
        if (EntityManager.HasBuffer<FlockPrefabSelection>(flockCenterEntity))
        {
            var selectionBuffer = EntityManager.GetBuffer<FlockPrefabSelection>(flockCenterEntity);
            var selectedPrefabs = new NativeList<Entity>(Allocator.Temp);

            Debug.Log($"FlockID {flockId} has {selectionBuffer.Length} prefab selection entries");

            foreach (var selection in selectionBuffer)
            {
                Debug.Log($"Processing selection - PrefabIndex: {selection.PrefabIndex}, PrefabEntity: {selection.PrefabEntity}");

                // If direct entity reference is set, use it
                if (selection.PrefabEntity != Entity.Null)
                {
                    selectedPrefabs.Add(selection.PrefabEntity);
                    Debug.Log($"Added direct prefab entity: {selection.PrefabEntity}");
                }
                // If index is -1, use all prefabs
                else if (selection.PrefabIndex == -1)
                {
                    Debug.Log("Using all prefabs (index -1)");
                    for (int i = 0; i < allPrefabs.Length; i++)
                    {
                        selectedPrefabs.Add(allPrefabs[i]);
                    }
                    break; // Don't add more since we're using all
                }
                // Use specific index
                else if (selection.PrefabIndex >= 0 && selection.PrefabIndex < allPrefabs.Length)
                {
                    selectedPrefabs.Add(allPrefabs[selection.PrefabIndex]);
                    Debug.Log($"Added prefab at index {selection.PrefabIndex}: {allPrefabs[selection.PrefabIndex]}");
                }
                else
                {
                    Debug.LogWarning($"Invalid prefab index {selection.PrefabIndex} for FlockID {flockId}. Available indices: 0-{allPrefabs.Length - 1}");
                }
            }

            if (selectedPrefabs.Length > 0)
            {
                var result = new NativeArray<Entity>(selectedPrefabs.Length, Allocator.Temp);
                for (int i = 0; i < selectedPrefabs.Length; i++)
                {
                    result[i] = selectedPrefabs[i];
                }
                selectedPrefabs.Dispose();
                allPrefabs.Dispose();

                Debug.Log($"FlockID {flockId} will use {result.Length} selected prefabs");
                return result;
            }
            else
            {
                Debug.LogWarning($"No valid prefabs selected for FlockID {flockId}, falling back to all prefabs");
                selectedPrefabs.Dispose();
            }
        }
        else
        {
            Debug.Log($"FlockID {flockId} has no prefab selection buffer, using all prefabs");
        }

        // Fallback: use all available prefabs
        return allPrefabs;
    }

    private NativeArray<Entity> GetAllAvailablePrefabs()
    {
        // First priority: Try to get from registry buffer (this has ALL prefabs)
        var registryQuery = SystemAPI.QueryBuilder()
            .WithAll<FishPrefabReference>()
            .Build();

        if (!registryQuery.IsEmpty)
        {
            var registryEntity = registryQuery.GetSingletonEntity();
            var buffer = EntityManager.GetBuffer<FishPrefabReference>(registryEntity);

            if (buffer.Length > 0)
            {
                var prefabs = new NativeArray<Entity>(buffer.Length, Allocator.Temp);
                for (int i = 0; i < buffer.Length; i++)
                {
                    prefabs[i] = buffer[i].Prefab;
                }
                Debug.Log($"Found {buffer.Length} prefabs in registry");
                return prefabs;
            }
        }

        // Fallback: Try to get from FishPrefabComponent (single prefab only)
        if (SystemAPI.HasSingleton<FishPrefabComponent>())
        {
            var singlePrefab = new NativeArray<Entity>(1, Allocator.Temp);
            singlePrefab[0] = SystemAPI.GetSingleton<FishPrefabComponent>().prefab;
            Debug.Log("Using single prefab from FishPrefabComponent");
            return singlePrefab;
        }

        // Return empty array if no prefabs found
        Debug.LogWarning("No prefabs found in registry or FishPrefabComponent");
        return new NativeArray<Entity>(0, Allocator.Temp);
    }

    private float3 GetFlockCenterPosition(int flockId, float3 defaultPosition)
    {
        var query = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        if (!query.IsEmpty)
        {
            var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
            var flockCenters = query.ToComponentDataArray<FlockCenterData>(Allocator.Temp);

            for (int i = 0; i < flockIDs.Length; i++)
            {
                if (flockIDs[i].FlockID == flockId)
                {
                    var result = flockCenters[i].Position;
                    flockIDs.Dispose();
                    flockCenters.Dispose();
                    return result;
                }
            }

            flockIDs.Dispose();
            flockCenters.Dispose();
        }

        return defaultPosition;
    }

    private FishControllerSettingsComponent GetDefaultSettings()
    {
        return new FishControllerSettingsComponent
        {
            SpawnCount = 500,
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
    }

    // Remove Scene ownership from an entity and all linked children, and tag them as Overworld
    private static void MakePersistentHierarchy(Entity root, EntityManager em)
    {
        // Gather the whole hierarchy (root + children) via LinkedEntityGroup if present
        if (em.HasBuffer<LinkedEntityGroup>(root))
        {
            var group = em.GetBuffer<LinkedEntityGroup>(root);
            for (int i = 0; i < group.Length; i++)
            {
                MakePersistentEntity(group[i].Value, em);
            }
        }
        else
        {
            // Single-entity prefab case
            MakePersistentEntity(root, em);
        }
    }

    private static void MakePersistentEntity(Entity e, EntityManager em)
    {
        // Strip scene ownership so unloads don't kill them
        if (em.HasComponent<SceneTag>(e)) em.RemoveComponent<SceneTag>(e);
        if (em.HasComponent<SceneSection>(e)) em.RemoveComponent<SceneSection>(e);

        // Add your own domain tag for explicit control
        if (!em.HasComponent<OverworldTag>(e)) em.AddComponent<OverworldTag>(e);
    }

    protected override void OnUpdate()
    {
        // Check for new flock centers and spawn boids if needed
        var query = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        if (!query.IsEmpty)
        {
            var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

            for (int i = 0; i < flockIDs.Length; i++)
            {
                int flockId = flockIDs[i].FlockID;
                if (!_spawnedFlocks.Contains(flockId))
                {
                    SpawnFish(flockId);
                    _spawnedFlocks.Add(flockId);
                    Debug.Log($"Runtime spawned boids for new FlockID {flockId}");
                }
            }

            flockIDs.Dispose();
        }
    }
}
public struct OverworldTag : IComponentData { }
