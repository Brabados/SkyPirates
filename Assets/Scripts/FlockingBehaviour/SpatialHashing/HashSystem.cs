using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;


[UpdateInGroup(typeof(SimulationSystemGroup))]

public partial struct OptimizedSpatialHashSystem : ISystem
{
    private NativeParallelMultiHashMap<int3, BoidData> _spatialMap;
    private NativeArray<BoidData> _boidDataArray;
    private NativeArray<Entity> _entityArray;
    private int _lastBoidCount;

    public struct BoidData
    {
        public float3 Position;
        public float3 Velocity;
        public Entity Entity;
        public int Index;
    }

    public void OnCreate(ref SystemState state)
    {
        _spatialMap = new NativeParallelMultiHashMap<int3, BoidData>(8192, Allocator.Persistent);
        _boidDataArray = new NativeArray<BoidData>(8192, Allocator.Persistent);
        _entityArray = new NativeArray<Entity>(8192, Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_spatialMap.IsCreated) _spatialMap.Dispose();
        if (_boidDataArray.IsCreated) _boidDataArray.Dispose();
        if (_entityArray.IsCreated) _entityArray.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _spatialMap.Clear();

        // Count boids first
        var boidQuery = SystemAPI.QueryBuilder().WithAll<BoidTag, LocalTransform, Velocity>().Build();
        int boidCount = boidQuery.CalculateEntityCount();
        _lastBoidCount = boidCount;

        if (boidCount == 0) return;

        // Resize arrays if needed
        if (_boidDataArray.Length < boidCount)
        {
            _boidDataArray.Dispose();
            _entityArray.Dispose();
            int newSize = math.max(boidCount * 2, 1024);
            _boidDataArray = new NativeArray<BoidData>(newSize, Allocator.Persistent);
            _entityArray = new NativeArray<Entity>(newSize, Allocator.Persistent);
        }

        // Populate spatial hash job (scheduled, not completed here)
        var populateJob = new PopulateSpatialHashJob
        {
            SpatialMap = _spatialMap.AsParallelWriter(),
            BoidDataArray = _boidDataArray,
            EntityArray = _entityArray,
            CellSize = 5f // Match search radius
        };

        // Schedule the populate job and assign its handle to the system dependency
        var populateHandle = populateJob.ScheduleParallel(boidQuery, state.Dependency);
        state.Dependency = populateHandle;

    }

    public NativeParallelMultiHashMap<int3, BoidData> GetSpatialMap() => _spatialMap;
    public NativeArray<BoidData> GetBoidDataArray() => _boidDataArray;
    public NativeArray<Entity> GetEntityArray() => _entityArray;
    public int GetLastBoidCount() => _lastBoidCount;
}

[BurstCompile]
partial struct PopulateSpatialHashJob : IJobEntity
{
    public NativeParallelMultiHashMap<int3, OptimizedSpatialHashSystem.BoidData>.ParallelWriter SpatialMap;
    [NativeDisableParallelForRestriction]
    public NativeArray<OptimizedSpatialHashSystem.BoidData> BoidDataArray;
    [NativeDisableParallelForRestriction]
    public NativeArray<Entity> EntityArray;
    public float CellSize;

    void Execute(Entity entity, [EntityIndexInQuery] int index, in LocalTransform transform, in Velocity velocity)
    {
        var boidData = new OptimizedSpatialHashSystem.BoidData
        {
            Position = transform.Position,
            Velocity = velocity.Value,
            Entity = entity,
            Index = index
        };

        BoidDataArray[index] = boidData;
        EntityArray[index] = entity;

        int3 cellCoord = SpatialHashUtils.GetSpatialHash(transform.Position, CellSize);
        SpatialMap.Add(cellCoord, boidData);
    }
}
