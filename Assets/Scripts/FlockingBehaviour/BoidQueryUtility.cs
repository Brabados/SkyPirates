using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public static class BoidQueryUtility
{
    private static EntityQuery _cachedQuery;
    private static World _cachedWorld;

    public static bool HasBoidsWithinDistance(
        int flockID,
        Vector3 centerPoint,
        float maxDistance)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return false;

        var em = world.EntityManager;

        // Cache the query to avoid creating it every time.
        // If the world changes, we need to recreate the query for the new world.
        if (_cachedWorld != world)
        {
            _cachedQuery = em.CreateEntityQuery(
                ComponentType.ReadOnly<BoidTag>(),
                ComponentType.ReadOnly<BoidFlockID>(),
                ComponentType.ReadOnly<LocalTransform>()
            );
            _cachedWorld = world;
        }

        if (_cachedQuery.IsEmpty)
        {
            return false;
        }

        float maxDistanceSq = maxDistance * maxDistance;
        float3 center = centerPoint;

        // Use chunk iteration to avoid allocating arrays for all entities.
        var flockIDHandle = em.GetComponentTypeHandle<BoidFlockID>(true);
        var transformHandle = em.GetComponentTypeHandle<LocalTransform>(true);

        var chunks = _cachedQuery.ToArchetypeChunkArray(Allocator.Temp);
        bool found = false;

        for (int i = 0; i < chunks.Length; i++)
        {
            var chunk = chunks[i];
            var flockIDs = chunk.GetNativeArray(ref flockIDHandle);
            var transforms = chunk.GetNativeArray(ref transformHandle);

            for (int j = 0; j < chunk.Count; j++)
            {
                if (flockIDs[j].FlockID != flockID)
                    continue;

                float distSq = math.lengthsq(transforms[j].Position - center);
                if (distSq <= maxDistanceSq)
                {
                    found = true;
                    break;
                }
            }

            if (found)
                break;
        }

        chunks.Dispose();

        return found;
    }
}
