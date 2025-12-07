using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Simple script to sync GameObject position to existing flock center entity.
/// Add this ALONGSIDE FlockCenterAuthoring on moving objects.
/// </summary>
public class SimpleFlockPositionSync : MonoBehaviour
{
    [Header("Settings")]
    public int flockID = 0; // Must match the FlockID in FlockCenterAuthoring

    private Entity _flockEntity = Entity.Null;
    private EntityManager _entityManager;
    private float3 _lastPosition;
    private bool _found = false;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _lastPosition = transform.position;

        // Wait a frame for baking to complete, then find the entity
        Invoke(nameof(FindFlockEntity), 0.1f);
    }

    void Update()
    {
        if (!_found) return;

        if (_flockEntity == Entity.Null || !_entityManager.Exists(_flockEntity))
        {
            FindFlockEntity();
            return;
        }

        float3 currentPosition = transform.position;

        // Only update if position changed significantly
        if (math.distance(_lastPosition, currentPosition) > 0.01f)
        {
            // Directly update the FlockCenterData position
            if (_entityManager.HasComponent<FlockCenterData>(_flockEntity))
            {
                var flockData = _entityManager.GetComponentData<FlockCenterData>(_flockEntity);
                flockData.Position = currentPosition;
                _entityManager.SetComponentData(_flockEntity, flockData);
            }

            _lastPosition = currentPosition;
        }
    }

    private void FindFlockEntity()
    {
        var query = _entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<FlockCenterData>(),
            ComponentType.ReadOnly<BoidFlockID>()
        );

        if (query.IsEmpty)
        {
            query.Dispose();
            return;
        }

        var entities = query.ToEntityArray(Allocator.Temp);
        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);

        for (int i = 0; i < flockIDs.Length; i++)
        {
            if (flockIDs[i].FlockID == flockID)
            {
                _flockEntity = entities[i];
                _found = true;
                Debug.Log($"Found flock entity for FlockID {flockID}: {_flockEntity}");
                break;
            }
        }

        entities.Dispose();
        flockIDs.Dispose();
        query.Dispose();

        if (!_found)
        {
            Debug.LogWarning($"Could not find flock entity for FlockID {flockID}");
        }
    }
}
