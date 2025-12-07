using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct FlockCenterBootstrapSystem : ISystem
{
    private bool _hasInitialized;

    public void OnCreate(ref SystemState state)
    {
        _hasInitialized = false;
    }

    public void OnUpdate(ref SystemState state)
    {
        // Only run once during initialization
        if (_hasInitialized) return;


        // --- 2. Spawn flock centers for all FlockCenterAuthoring objects in scene ---
        foreach (var authoring in Object.FindObjectsOfType<FlockCenterAuthoring>())
        {
            int flockID = authoring.FlockID;
            if (!HasFlockCenterEntity(ref state, flockID))
            {
                Entity flockEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(flockEntity, new FlockCenterData
                {
                    FlockID = flockID, // Make sure to set the FlockID in the FlockCenterData
                    Position = authoring.transform.position
                });
                state.EntityManager.AddComponentData(flockEntity, new BoidFlockID { FlockID = flockID });

                // Add prefab selection buffer
                var prefabBuffer = state.EntityManager.AddBuffer<FlockPrefabSelection>(flockEntity);

                // If specific prefabs are assigned, use those
                if (authoring.SpecificPrefabs.Length > 0)
                {
                    foreach (var prefab in authoring.SpecificPrefabs)
                    {
                        if (prefab != null)
                        {
                            // Note: We can't get entity references here in bootstrap since baking hasn't happened
                            // So we'll just mark it to use all prefabs for now
                            prefabBuffer.Add(new FlockPrefabSelection { PrefabIndex = -1, PrefabEntity = Entity.Null });
                            break; // Just add one entry to mark that this flock exists
                        }
                    }
                }
                // Use indices from the registry
                else if (authoring.AllowedPrefabIndices.Length > 0)
                {
                    foreach (int index in authoring.AllowedPrefabIndices)
                    {
                        prefabBuffer.Add(new FlockPrefabSelection { PrefabIndex = index, PrefabEntity = Entity.Null });
                    }
                }
                // If nothing specified, mark as "use all" with index -1
                else
                {
                    prefabBuffer.Add(new FlockPrefabSelection { PrefabIndex = -1, PrefabEntity = Entity.Null });
                }

#if UNITY_EDITOR
                state.EntityManager.SetName(flockEntity, $"FlockCenterEntity_{flockID}");
#endif

                Debug.Log($"Created flock center for FlockID {flockID} at {authoring.transform.position}");
            }
        }

        _hasInitialized = true;

        // Log all existing flock centers for debugging
        LogAllFlockCenters(ref state);
    }

    private bool HasFlockCenterEntity(ref SystemState state, int flockID)
    {
        // Create a query for entities with both FlockCenterData and BoidFlockID
        var query = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        // Check if any entities exist with this FlockID
        var entities = query.ToEntityArray(Allocator.Temp);
        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

        bool found = false;
        for (int i = 0; i < flockIDs.Length; i++)
        {
            if (flockIDs[i].FlockID == flockID)
            {
                found = true;
                break;
            }
        }

        entities.Dispose();
        flockIDs.Dispose();
        return found;
    }

    private void LogAllFlockCenters(ref SystemState state)
    {
        var query = SystemAPI.QueryBuilder()
            .WithAll<FlockCenterData, BoidFlockID>()
            .Build();

        var entities = query.ToEntityArray(Allocator.Temp);
        var flockCenters = query.ToComponentDataArray<FlockCenterData>(Allocator.Temp);
        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

        Debug.Log($"Total flock centers created: {entities.Length}");
        for (int i = 0; i < entities.Length; i++)
        {
            Debug.Log($"Flock Center - FlockID: {flockIDs[i].FlockID}, Position: {flockCenters[i].Position}, Entity: {entities[i]}");
        }

        entities.Dispose();
        flockCenters.Dispose();
        flockIDs.Dispose();
    }
}
