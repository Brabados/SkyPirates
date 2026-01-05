using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public static class BoidQueryUtility
{
    public static bool HasBoidsWithinDistance(
        int flockID,
        Vector3 centerPoint,
        float maxDistance)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return false;

        var em = world.EntityManager;

        var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<BoidTag>(),
            ComponentType.ReadOnly<BoidFlockID>(),
            ComponentType.ReadOnly<LocalTransform>()
        );

        if (query.IsEmpty)
        {
            query.Dispose();
            return false;
        }

        float maxDistanceSq = maxDistance * maxDistance;
        float3 center = centerPoint;

        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
        var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        bool found = false;

        for (int i = 0; i < flockIDs.Length; i++)
        {
            if (flockIDs[i].FlockID != flockID)
                continue;

            float distSq = math.lengthsq(transforms[i].Position - center);
            if (distSq <= maxDistanceSq)
            {
                found = true;
                break;
            }
        }

        flockIDs.Dispose();
        transforms.Dispose();
        query.Dispose();

        return found;
    }
}
