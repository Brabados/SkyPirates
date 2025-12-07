using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(OptimizedFlockingBehaviorSystem))]
public partial struct BoundaryUpdateSystem : ISystem
{
    private double _lastLogTime;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidTag>();
        state.RequireForUpdate<FlockCenterData>();
        _lastLogTime = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Build a lookup of current flock center positions
        var flockCenterLookup = new NativeParallelHashMap<int, float3>(16, Allocator.TempJob);

        // Log flock center positions periodically
        bool shouldLog = SystemAPI.Time.ElapsedTime - _lastLogTime > 1.0; // Every second
        if (shouldLog)
        {
            _lastLogTime = SystemAPI.Time.ElapsedTime;
        }

        // Populate the lookup with current flock center positions
        foreach (var flockCenter in SystemAPI.Query<RefRO<FlockCenterData>>())
        {
            flockCenterLookup.TryAdd(flockCenter.ValueRO.FlockID, flockCenter.ValueRO.Position);

            if (shouldLog)
            {
                Debug.Log($"BoundaryUpdateSystem: FlockID {flockCenter.ValueRO.FlockID} center at {flockCenter.ValueRO.Position}");
            }
        }

        // Update all boid boundary settings based on their flock ID
        var updateJob = new UpdateBoundarySettingsJob
        {
            FlockCenterLookup = flockCenterLookup.AsReadOnly(),
            ShouldLog = shouldLog,
            ElapsedTime = SystemAPI.Time.ElapsedTime
        };

        var jobHandle = updateJob.ScheduleParallel(state.Dependency);
        jobHandle = flockCenterLookup.Dispose(jobHandle);
        state.Dependency = jobHandle;
    }
}

[BurstCompile]
public partial struct UpdateBoundarySettingsJob : IJobEntity
{
    [ReadOnly] public NativeParallelHashMap<int, float3>.ReadOnly FlockCenterLookup;
    [ReadOnly] public bool ShouldLog;
    [ReadOnly] public double ElapsedTime;

    void Execute([EntityIndexInQuery] int index, ref BoundarySettings boundarySettings, in BoidFlockID flockID)
    {
        // Update the boundary center if we find a matching flock center
        if (FlockCenterLookup.TryGetValue(flockID.FlockID, out float3 newCenter))
        {
            float3 oldCenter = boundarySettings.Center;
            float distance = math.distance(oldCenter, newCenter);

            // Only update if there's a meaningful change
            if (distance > 0.01f)
            {
                boundarySettings.Center = newCenter;

                // Log only for the first few boids to avoid spam
                if (ShouldLog && index < 3)
                {
                    //Debug.Log($"BoundaryUpdateSystem: Updated boid boundary from {oldCenter} to {newCenter} (distance: {distance:F2})");
                }
            }
        }
    }
}
