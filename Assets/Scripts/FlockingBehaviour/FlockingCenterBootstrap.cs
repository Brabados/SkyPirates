using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// FIXED: This system now properly detects baked FlockCenterData entities
/// and creates FlockRepresentatives for them.
/// </summary>
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct FlockCenterBootstrapSystem : ISystem
{
    private bool _hasInitialized;
    private EntityQuery _flockCenterQuery;
    private NativeHashSet<int> _knownFlocks;

    public void OnCreate(ref SystemState state)
    {
        _hasInitialized = false;
        _knownFlocks = new NativeHashSet<int>(16, Allocator.Persistent);

        // Create a query to check for flock centers
        _flockCenterQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<FlockCenterData>(),
            ComponentType.ReadOnly<BoidFlockID>()
        );


        Debug.Log("FlockCenterBootstrapSystem created");
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_knownFlocks.IsCreated)
            _knownFlocks.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (_flockCenterQuery.IsEmpty)
            return;

        var flockCenters = _flockCenterQuery.ToComponentDataArray<FlockCenterData>(Allocator.Temp);
        var flockIDs = _flockCenterQuery.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

        for (int i = 0; i < flockCenters.Length; i++)
        {
            int flockID = flockIDs[i].FlockID;

            // Already has a representative → skip
            if (_knownFlocks.Contains(flockID))
                continue;

            CreateFlockRepresentative(flockID, flockCenters[i].Position);
            _knownFlocks.Add(flockID);
        }

        flockCenters.Dispose();
        flockIDs.Dispose();
    }


    /// <summary>
    /// Creates a FlockRepresentative GameObject for targeting at runtime
    /// </summary>
    private void CreateFlockRepresentative(int flockID, Vector3 position)
    {
        // Check if representative already exists
        var existing = GameObject.Find($"FlockRepresentative_{flockID}");
        if (existing != null)
        {
            Debug.Log($"FlockRepresentative_{flockID} already exists, skipping creation");
            return;
        }

        // Create the representative GameObject
        GameObject representative = new GameObject($"FlockRepresentative_{flockID}");
        representative.transform.position = position;

        // Add and configure FlockRepresentative component
        var repComponent = representative.AddComponent<FlockRepresentative>();
        repComponent.flockID = flockID;

        Debug.Log($"Created FlockRepresentative for FlockID {flockID} at {position}");
    }

    private void LogAllFlockCenters(ref SystemState state)
    {
        var entities = _flockCenterQuery.ToEntityArray(Allocator.Temp);
        var flockCenters = _flockCenterQuery.ToComponentDataArray<FlockCenterData>(Allocator.Temp);
        var flockIDs = _flockCenterQuery.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

        Debug.Log($"=== Total flock centers: {entities.Length} ===");
        for (int i = 0; i < entities.Length; i++)
        {
            Debug.Log($"Flock Center - FlockID: {flockIDs[i].FlockID}, Position: {flockCenters[i].Position}, Entity: {entities[i]}");
        }

        entities.Dispose();
        flockCenters.Dispose();
        flockIDs.Dispose();
    }
}
