using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Creates a new flock center dynamically when this GameObject is spawned.
/// Use this INSTEAD of FlockCenterAuthoring on prefabs.
/// </summary>
public class DynamicFlockCreator : MonoBehaviour
{
    [Header("Flock Settings")]
    [Tooltip("Leave at -1 to auto-assign unique FlockID")]
    public int flockID = -1;

    [Header("Boid Settings")]
    public int boidCount = 500;
    public float spawnRadius = 10f;
    public float boundryRadius = 10f;
    public float boundaryWeight = 2f;
    public float initialSpeed = 2f;
    public float maxSpeed = 5f;
    public float searchRadius = 5f;
    public float cohesionWeight = 1f;
    public float alignmentWeight = 1f;
    public float separationWeight = 1f;
    public float obstacleAvoidanceDistance = 2f;
    public float obstacleAvoidanceWeight = 1f;


    [Header("Prefab Selection")]
    [Tooltip("Leave empty to use all available prefabs")]
    public int[] allowedPrefabIndices = new int[0];

    private Entity _flockEntity = Entity.Null;
    private EntityManager _entityManager;
    private bool _initialized = false;
    private float3 _lastPosition;

    // Static counter to ensure unique FlockIDs
    private static int _nextFlockID = 100; // Start high to avoid conflicts with scene-based flocks

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _lastPosition = transform.position;

        CreateFlockEntity();
        SpawnBoids();
        _initialized = true;
    }

    void Update()
    {
        if (!_initialized || _flockEntity == Entity.Null) return;
        if (!_entityManager.Exists(_flockEntity)) return;

        float3 currentPosition = transform.position;

        if (math.distance(_lastPosition, currentPosition) > 0.01f)
        {
            // Update flock center position
            if (_entityManager.HasComponent<FlockCenterData>(_flockEntity))
            {
                var flockData = _entityManager.GetComponentData<FlockCenterData>(_flockEntity);
                flockData.Position = currentPosition;
                _entityManager.SetComponentData(_flockEntity, flockData);
            }

            _lastPosition = currentPosition;
        }
    }

    private void CreateFlockEntity()
    {
        // Assign unique FlockID if not set
        if (flockID == -1)
        {
            flockID = GetNextAvailableFlockID();
        }

        // Create the flock center entity
        _flockEntity = _entityManager.CreateEntity();

        _entityManager.AddComponentData(_flockEntity, new FlockCenterData
        {
            FlockID = flockID,
            Position = transform.position
        });

        _entityManager.AddComponentData(_flockEntity, new BoidFlockID { FlockID = flockID });

        // Add prefab selection
        var prefabBuffer = _entityManager.AddBuffer<FlockPrefabSelection>(_flockEntity);
        if (allowedPrefabIndices.Length > 0)
        {
            foreach (int index in allowedPrefabIndices)
            {
                prefabBuffer.Add(new FlockPrefabSelection { PrefabIndex = index, PrefabEntity = Entity.Null });
            }
        }
        else
        {
            // Use all prefabs
            prefabBuffer.Add(new FlockPrefabSelection { PrefabIndex = -1, PrefabEntity = Entity.Null });
        }

#if UNITY_EDITOR
        _entityManager.SetName(_flockEntity, $"DynamicFlockCenter_{flockID}");
#endif

        Debug.Log($"Created dynamic flock center for FlockID {flockID} at {transform.position}");
    }

    private void SpawnBoids()
    {
        // Use the existing FishFlockController to spawn boids for this flock
        var controller = FishFlockController.Instance;
        if (controller != null)
        {
            // Override the spawn count temporarily
            OverrideSpawnSettings();

            controller.TriggerSpawn(flockID);
            Debug.Log($"Spawned {boidCount} boids for dynamic FlockID {flockID}");
        }
        else
        {
            Debug.LogWarning("FishFlockController not available for dynamic flock spawning");
        }
    }

    private void OverrideSpawnSettings()
    {
        // Check if settings entity already exists using EntityManager
        var settingsQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<FishControllerSettingsComponent>());

        if (settingsQuery.IsEmpty)
        {
            var settingsEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(settingsEntity, new FishControllerSettingsComponent
            {
                SpawnCount = boidCount,
                SpawnRadius = spawnRadius,
                InitialSpeed = initialSpeed,
                MaxSpeed = maxSpeed,
                SearchRadius = searchRadius,
                CohesionWeight = cohesionWeight,
                AlignmentWeight = alignmentWeight,
                SeparationWeight = separationWeight,
                ObstacleAvoidanceDistance = obstacleAvoidanceDistance,
                ObstacleAvoidanceWeight = obstacleAvoidanceWeight,
                BoundaryCenter = transform.position,
                BoundaryRadius = boundryRadius,
                BoundaryWeight = boundaryWeight
            });

            Debug.Log("Created FishControllerSettingsComponent for dynamic flock");
        }
        else
        {
            // CRITICAL FIX: Check if there are multiple entities and clean them up
            int count = settingsQuery.CalculateEntityCount();
            if (count > 1)
            {
                Debug.LogWarning($"Found {count} FishControllerSettingsComponent entities, removing duplicates!");
                var entities = settingsQuery.ToEntityArray(Allocator.Temp);

                // Keep the first one, destroy the rest
                for (int i = 1; i < entities.Length; i++)
                {
                    _entityManager.DestroyEntity(entities[i]);
                }

                entities.Dispose();
            }

            // Update the remaining settings
            var settingsEntity = settingsQuery.GetSingletonEntity();
            var settings = _entityManager.GetComponentData<FishControllerSettingsComponent>(settingsEntity);
            settings.SpawnCount = boidCount;
            settings.SpawnRadius = spawnRadius;
            settings.BoundaryCenter = transform.position;
            settings.BoundaryRadius = boundryRadius;
            _entityManager.SetComponentData(settingsEntity, settings);

            Debug.Log($"Updated existing FishControllerSettingsComponent with boid count: {boidCount}");
        }

        settingsQuery.Dispose();
    }

    private int GetNextAvailableFlockID()
    {
        // Find the highest existing FlockID and add 1
        var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<BoidFlockID>());

        if (query.IsEmpty)
        {
            query.Dispose();
            return _nextFlockID++;
        }

        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
        int maxID = 0;

        for (int i = 0; i < flockIDs.Length; i++)
        {
            if (flockIDs[i].FlockID > maxID)
                maxID = flockIDs[i].FlockID;
        }

        flockIDs.Dispose();
        query.Dispose();

        int newID = math.max(maxID + 1, _nextFlockID);
        _nextFlockID = newID + 1;

        return newID;
    }

    public int GetFlockID() => flockID;

    /// <summary>
    /// Manually clean up this flock and all its boids.
    /// Call this before destroying the GameObject if you want guaranteed cleanup.
    /// </summary>
    public void CleanupFlock()
    {
        CleanupFlockBoids();

        if (_flockEntity != Entity.Null &&
            _entityManager != null &&
            World.DefaultGameObjectInjectionWorld != null &&
            World.DefaultGameObjectInjectionWorld.IsCreated &&
            _entityManager.Exists(_flockEntity))
        {
            _entityManager.DestroyEntity(_flockEntity);
            _flockEntity = Entity.Null;
            Debug.Log($"Manually cleaned up flock FlockID {flockID}");
        }
    }

    /// <summary>
    /// Check if any boids from this flock are within the specified distance from a center point.
    /// </summary>
    /// <param name="centerPoint">The center point to check distance from</param>
    /// <param name="maxDistance">Maximum distance to consider</param>
    /// <returns>True if any boids are within the distance, false otherwise</returns>
    public bool HasBoidsWithinDistance(Vector3 centerPoint, float maxDistance)
    {
        if (!_initialized || _entityManager == null || flockID == -1)
            return false;

        try
        {
            var boidQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<BoidTag>(),
                ComponentType.ReadOnly<BoidFlockID>(),
                ComponentType.ReadOnly<Unity.Transforms.LocalTransform>()
            );

            if (boidQuery.IsEmpty)
            {
                boidQuery.Dispose();
                return false;
            }

            var entities = boidQuery.ToEntityArray(Allocator.Temp);
            var flockIDs = boidQuery.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
            var transforms = boidQuery.ToComponentDataArray<Unity.Transforms.LocalTransform>(Allocator.Temp);

            float maxDistanceSquared = maxDistance * maxDistance;
            bool foundWithinDistance = false;
            float3 center = centerPoint;

            for (int i = 0; i < flockIDs.Length; i++)
            {
                if (flockIDs[i].FlockID == flockID)
                {
                    float3 boidPosition = transforms[i].Position;
                    float distanceSquared = math.lengthsq(boidPosition - center);

                    if (distanceSquared <= maxDistanceSquared)
                    {
                        foundWithinDistance = true;
                        break;
                    }
                }
            }

            entities.Dispose();
            flockIDs.Dispose();
            transforms.Dispose();
            boidQuery.Dispose();

            return foundWithinDistance;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error checking boid distances for FlockID {flockID}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if any boids from this flock are within the specified distance from this GameObject's position.
    /// </summary>
    /// <param name="maxDistance">Maximum distance to consider</param>
    /// <returns>True if any boids are within the distance, false otherwise</returns>
    public bool HasBoidsWithinDistance(float maxDistance)
    {
        return HasBoidsWithinDistance(transform.position, maxDistance);
    }

    /// <summary>
    /// Get the count of boids within a specified distance from a center point.
    /// </summary>
    /// <param name="centerPoint">The center point to check distance from</param>
    /// <param name="maxDistance">Maximum distance to consider</param>
    /// <returns>Number of boids within the distance</returns>
    public int GetBoidsCountWithinDistance(Vector3 centerPoint, float maxDistance)
    {
        if (!_initialized || _entityManager == null || flockID == -1)
            return 0;

        try
        {
            var boidQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<BoidTag>(),
                ComponentType.ReadOnly<BoidFlockID>(),
                ComponentType.ReadOnly<Unity.Transforms.LocalTransform>()
            );

            if (boidQuery.IsEmpty)
            {
                boidQuery.Dispose();
                return 0;
            }

            var entities = boidQuery.ToEntityArray(Allocator.Temp);
            var flockIDs = boidQuery.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
            var transforms = boidQuery.ToComponentDataArray<Unity.Transforms.LocalTransform>(Allocator.Temp);

            float maxDistanceSquared = maxDistance * maxDistance;
            int count = 0;
            float3 center = centerPoint;

            for (int i = 0; i < flockIDs.Length; i++)
            {
                if (flockIDs[i].FlockID == flockID)
                {
                    float3 boidPosition = transforms[i].Position;
                    float distanceSquared = math.lengthsq(boidPosition - center);

                    if (distanceSquared <= maxDistanceSquared)
                    {
                        count++;
                    }
                }
            }

            entities.Dispose();
            flockIDs.Dispose();
            transforms.Dispose();
            boidQuery.Dispose();

            return count;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error counting boids within distance for FlockID {flockID}: {e.Message}");
            return 0;
        }
    }

    void OnDestroy()
    {
        // Clean up all boids belonging to this flock first
        CleanupFlockBoids();

        // Then clean up the flock entity
        if (_flockEntity != Entity.Null &&
            _entityManager != null &&
            World.DefaultGameObjectInjectionWorld != null &&
            World.DefaultGameObjectInjectionWorld.IsCreated &&
            _entityManager.Exists(_flockEntity))
        {
            _entityManager.DestroyEntity(_flockEntity);
            Debug.Log($"Cleaned up dynamic flock center for FlockID {flockID}");
        }
    }

    private void CleanupFlockBoids()
    {
        if (!_initialized || _entityManager == null) return;

        try
        {
            // Find all boids belonging to this flock
            var boidQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<BoidTag>(),
                ComponentType.ReadOnly<BoidFlockID>()
            );

            if (boidQuery.IsEmpty)
            {
                boidQuery.Dispose();
                return;
            }

            var entities = boidQuery.ToEntityArray(Allocator.Temp);
            var flockIDs = boidQuery.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

            int destroyedCount = 0;

            for (int i = 0; i < flockIDs.Length; i++)
            {
                if (flockIDs[i].FlockID == flockID)
                {
                    _entityManager.DestroyEntity(entities[i]);
                    destroyedCount++;
                }
            }

            entities.Dispose();
            flockIDs.Dispose();
            boidQuery.Dispose();

            Debug.Log($"Destroyed {destroyedCount} boids for FlockID {flockID}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error cleaning up boids for FlockID {flockID}: {e.Message}");
        }
    }

    void OnDrawGizmos()
    {
        // Visualize the dynamic flock center
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);

        // Draw flock ID
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"FlockID: {flockID}");
#endif
    }
}
