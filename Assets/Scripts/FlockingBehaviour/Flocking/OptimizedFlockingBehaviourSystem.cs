using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(OptimizedSpatialHashSystem))]
[UpdateAfter(typeof(AssignBoidUpdateGroupsSystem))] // Ensure this runs after group assignment
public partial struct OptimizedFlockingBehaviorSystem : ISystem
{
    private double _lastShipUpdateTime;
    private int _currentUpdateGroup;
    private ComponentLookup<BoidUpdateGroup> _boidGroupLookup;

    public void OnCreate(ref SystemState state)
    {
        _boidGroupLookup = state.GetComponentLookup<BoidUpdateGroup>(isReadOnly: true);
    }

    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        double currentTime = SystemAPI.Time.ElapsedTime;
        int frameCount = Time.frameCount;

        var spatialHashSystem = state.WorldUnmanaged.GetExistingUnmanagedSystem<OptimizedSpatialHashSystem>();
        var spatialHashRef = state.WorldUnmanaged.GetUnsafeSystemRef<OptimizedSpatialHashSystem>(spatialHashSystem);
        var spatialMap = spatialHashRef.GetSpatialMap();

        if (spatialHashRef.GetLastBoidCount() == 0) return;

        var shipData = new ShipData { HasShip = false };

        if (currentTime - _lastShipUpdateTime > 0.2)
        {
            foreach (var (transform, tag) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ShipProxyTag>>())
            {
                shipData.HasShip = true;
                shipData.Position = transform.ValueRO.Position;
                shipData.Radius = 1f;
                _lastShipUpdateTime = currentTime;
                break;
            }

            if (SystemAPI.HasSingleton<CachedShipData>())
            {
                SystemAPI.SetSingleton(new CachedShipData { Data = shipData });
            }
            else
            {
                var shipEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(shipEntity, new CachedShipData { Data = shipData });
            }
        }
        else if (SystemAPI.HasSingleton<CachedShipData>())
        {
            shipData = SystemAPI.GetSingleton<CachedShipData>().Data;
        }

        _currentUpdateGroup = frameCount % 4;

        var boidQuery = SystemAPI.QueryBuilder()
            .WithAll<BoidTag, LocalTransform, Velocity, BoidSettings, BoundarySettings>()
            .Build();

        // Update the ComponentLookup right before scheduling the job
        _boidGroupLookup.Update(ref state);

        var flockingJob = new UltraOptimizedFlockingJob
        {
            SpatialMap = spatialMap,
            DeltaTime = deltaTime * 4f,
            ShipData = shipData,
            FrameCount = frameCount,
            CurrentUpdateGroup = _currentUpdateGroup,
            BoidUpdateGroupHandle = _boidGroupLookup
        };

        state.Dependency = flockingJob.ScheduleParallel(boidQuery, state.Dependency);
    }
}

public struct BoidUpdateGroup : IComponentData { public int Group; }
public struct CachedShipData : IComponentData { public ShipData Data; }
public struct ShipData { public bool HasShip; public float3 Position; public float Radius; }

[BurstCompile]
public partial struct UltraOptimizedFlockingJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<int3, OptimizedSpatialHashSystem.BoidData> SpatialMap;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public ShipData ShipData;
    [ReadOnly] public int FrameCount;
    [ReadOnly] public int CurrentUpdateGroup;
    [ReadOnly] public ComponentLookup<BoidUpdateGroup> BoidUpdateGroupHandle;

    void Execute(Entity entity, [EntityIndexInQuery] int index,
        ref LocalTransform transform, ref Velocity velocity,
        in BoidSettings settings, in BoundarySettings boundarySettings)
    {
        // Use a fallback mechanism in case ComponentLookup fails
        int group;
        if (BoidUpdateGroupHandle.HasComponent(entity))
        {
            group = BoidUpdateGroupHandle[entity].Group;
        }
        else
        {
            // Fallback to index-based grouping if ComponentLookup is not available
            group = index % 4;
        }

        if (group != CurrentUpdateGroup) return;

        float3 pos = transform.Position;
        if (math.any(math.isnan(pos))) return;

        float3 zero = float3.zero;
        float3 up = new float3(0, 1, 0);
        float3 steer = zero;
        float3 obstacle = zero;
        float3 boundaryForce = zero;

        var neighbors = new NativeArray<OptimizedSpatialHashSystem.BoidData>(48, Allocator.Temp);
        int neighborCount = GetNeighbors(pos, settings.SearchRadius, neighbors);
        float3 alignment = zero;
        float3 cohesion = zero;
        float3 separation = zero;
        int valid = 0;
        float radiusSq = settings.SearchRadius * settings.SearchRadius;

        for (int i = 0; i < neighborCount; i++)
        {
            var neighbor = neighbors[i];
            if (neighbor.Entity == entity) continue;

            float3 diff = pos - neighbor.Position;
            float distSq = math.lengthsq(diff);
            if (distSq > radiusSq || distSq < 0.0001f) continue;

            cohesion += neighbor.Position;
            alignment += neighbor.Velocity;
            separation += diff / (distSq + 0.01f);
            valid++;
        }

        if (valid > 0)
        {
            float inv = 1f / valid;
            steer =
                math.normalizesafe(cohesion * inv - pos) * settings.CohesionWeight +
                math.normalizesafe(alignment * inv - velocity.Value) * settings.AlignmentWeight +
                math.normalizesafe(separation) * settings.SeparationWeight;
        }

        bool avoiding = false;
        if (ShipData.HasShip && settings.ObstacleAvoidanceDistance > 0.001f)
        {
            float3 diff = pos - ShipData.Position;
            float distSq = math.lengthsq(diff);
            float avoidRadius = ShipData.Radius + settings.ObstacleAvoidanceDistance;
            float avoidRadiusSq = avoidRadius * avoidRadius;

            if (distSq < avoidRadiusSq && distSq > 0.000001f)
            {
                avoiding = true;
                float invDist = math.rsqrt(distSq);
                float strength = (avoidRadius - math.sqrt(distSq)) / avoidRadius;
                obstacle = diff * invDist * settings.ObstacleAvoidanceWeight * strength * 4f;
            }
        }

        boundaryForce = CalculateBoundaryForce(pos, boundarySettings);
        float3 totalForce = avoiding
            ? obstacle * 5f + steer * 0.3f + boundaryForce * 0.5f
            : steer + boundaryForce;

        float3 newVel = velocity.Value + totalForce * DeltaTime;

        float speedSq = math.lengthsq(newVel);
        float maxSpeed = settings.MaxSpeed;
        float maxSpeedSq = maxSpeed * maxSpeed;
        float minSpeed = maxSpeed * 0.3f;
        float minSpeedSq = minSpeed * minSpeed;

        if (speedSq > maxSpeedSq)
            newVel *= maxSpeed * math.rsqrt(speedSq);
        else if (speedSq < minSpeedSq && speedSq > 0.000001f)
            newVel *= minSpeed * math.rsqrt(speedSq);

        velocity.Value = newVel;

        if (speedSq > 0.001f)
        {
            float3 dir = math.normalizesafe(newVel);
            if (math.lengthsq(dir) > 0.0001f && !math.any(math.isnan(dir)))
            {
                transform.Rotation = quaternion.LookRotationSafe(dir, up);
            }
        }

        transform.Position += newVel * DeltaTime;
        neighbors.Dispose();
    }

    [BurstCompile]
    private int GetNeighbors(float3 pos, float radius, NativeArray<OptimizedSpatialHashSystem.BoidData> buffer)
    {
        float cellSize = 5f;
        int3 center = SpatialHashUtils.GetSpatialHash(pos, cellSize);
        float radiusSq = radius * radius;
        int count = 0;

        // how many cells to search in each axis
        int radiusInCells = (int)math.ceil(radius / cellSize);
        radiusInCells = math.max(1, radiusInCells);

        for (int x = -radiusInCells; x <= radiusInCells; x++)
            for (int y = -radiusInCells; y <= radiusInCells; y++)
                for (int z = -radiusInCells; z <= radiusInCells; z++)
                {
                    int3 cell = center + new int3(x, y, z);
                    if (SpatialMap.TryGetFirstValue(cell, out var data, out var iter))
                    {
                        do
                        {
                            if (count >= buffer.Length) return count;
                            float distSq = math.lengthsq(pos - data.Position);
                            if (distSq <= radiusSq) buffer[count++] = data;
                        }
                        while (SpatialMap.TryGetNextValue(out data, ref iter));
                    }
                }

        return count;

    }

    [BurstCompile]
    private float3 CalculateBoundaryForce(float3 pos, BoundarySettings boundary)
    {
        float3 diff = boundary.Center - pos;
        float distSq = math.lengthsq(diff);
        float innerRadiusSq = boundary.Radius * boundary.Radius * 0.49f;

        if (distSq > innerRadiusSq)
        {
            float dist = math.sqrt(distSq);
            float strength = math.saturate((dist - boundary.Radius * 0.7f) / (boundary.Radius * 0.3f));
            return math.normalizesafe(diff) * boundary.BoundaryWeight * strength;
        }

        return float3.zero;
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct AssignBoidUpdateGroupsSystem : ISystem
{
    private ComponentLookup<BoidUpdateGroup> _boidGroupLookup;

    public void OnCreate(ref SystemState state)
    {
        _boidGroupLookup = state.GetComponentLookup<BoidUpdateGroup>(isReadOnly: false);
    }

    public void OnUpdate(ref SystemState state)
    {
        _boidGroupLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        int groupCounter = 0;

        foreach (var (tag, entity) in SystemAPI.Query<RefRO<BoidTag>>().WithEntityAccess().WithNone<BoidUpdateGroup>())
        {
            ecb.AddComponent(entity, new BoidUpdateGroup { Group = groupCounter % 4 });
            groupCounter++;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        // Only disable if no more entities need group assignment
        var ungroupedQuery = SystemAPI.QueryBuilder().WithAll<BoidTag>().WithNone<BoidUpdateGroup>().Build();
        if (ungroupedQuery.IsEmpty)
        {
            state.Enabled = false;
        }
    }
}
