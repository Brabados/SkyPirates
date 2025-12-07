using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct OptimizedSpatialHashSystem : ISystem
{
    public struct BoidData
    {
        public Entity Entity;
        public float3 Position;
        public float3 Velocity;
    }

    private NativeParallelMultiHashMap<int3, BoidData> _spatialMap;
    private NativeParallelHashSet<int3> _touchedCells;

    private int _lastCompletedBoidCount;
    private int _requestedBoidCount;
    private int _rebuildTargetCount;
    private int _populateCursor;
    private bool _isRebuilding;

    private const float CellSize = 5f;
    private const int rebuildFrames = 4;
    private int _batchSize;

    public NativeParallelHashSet<int3> GetTouchedCells() => _touchedCells;
    public void OnCreate(ref SystemState state)
    {
        _spatialMap = new NativeParallelMultiHashMap<int3, BoidData>(1024, Allocator.Persistent);
        _touchedCells = new NativeParallelHashSet<int3>(256, Allocator.Persistent);

        _lastCompletedBoidCount = 0;
        _requestedBoidCount = 0;
        _rebuildTargetCount = 0;
        _populateCursor = 0;
        _isRebuilding = false;
        _batchSize = 256;
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_spatialMap.IsCreated) _spatialMap.Dispose();
        if (_touchedCells.IsCreated) _touchedCells.Dispose();
    }

    public NativeParallelMultiHashMap<int3, BoidData> GetSpatialMap() => _spatialMap;
    public int GetLastBoidCount() => _lastCompletedBoidCount;

    public void OnUpdate(ref SystemState state)
    {
        var boidQuery = SystemAPI.QueryBuilder()
            .WithAll<BoidTag, LocalTransform, Velocity>()
            .Build();

        int currentBoidCount = boidQuery.CalculateEntityCount();
        _requestedBoidCount = currentBoidCount;

        if (currentBoidCount == 0)
        {
            _lastCompletedBoidCount = 0;
            _isRebuilding = false;
            _rebuildTargetCount = 0;
            _populateCursor = 0;
            return;
        }

        if (!_isRebuilding && currentBoidCount != _lastCompletedBoidCount)
        {
            _isRebuilding = true;
            _rebuildTargetCount = currentBoidCount;
            _populateCursor = 0;
            _batchSize = math.max(1, (int)math.ceil((float)_rebuildTargetCount / rebuildFrames));

            _spatialMap.Clear();
            _touchedCells.Clear();

            int requiredCapacity = _rebuildTargetCount * 4; // extra headroom for collisions
            if (_spatialMap.Capacity < requiredCapacity)
                _spatialMap.Capacity = requiredCapacity;

            if (_touchedCells.Capacity < _rebuildTargetCount)
                _touchedCells.Capacity = _rebuildTargetCount;
        }

        if (_isRebuilding)
        {
            // Ensure capacity BEFORE every chunk
            int requiredCapacity = _rebuildTargetCount * 4;
            if (_spatialMap.Capacity < requiredCapacity)
                _spatialMap.Capacity = requiredCapacity;

            if (_touchedCells.Capacity < _rebuildTargetCount)
                _touchedCells.Capacity = _rebuildTargetCount;

            int startIndex = _populateCursor;
            int remaining = _rebuildTargetCount - _populateCursor;
            int countThisBatch = math.min(_batchSize, remaining);

            if (countThisBatch <= 0)
            {
                FinishRebuild();
                return;
            }

            var spatialMapWriter = _spatialMap.AsParallelWriter();
            var touchedCellsWriter = _touchedCells.AsParallelWriter();

            var job = new PopulateSpatialHashChunkJob
            {
                SpatialMap = spatialMapWriter,
                TouchedCells = touchedCellsWriter,
                CellSize = CellSize,
                StartIndex = startIndex,
                Count = countThisBatch
            };

            var handle = job.ScheduleParallel(boidQuery, state.Dependency);
            handle.Complete();

            _populateCursor += countThisBatch;

            if (_populateCursor >= _rebuildTargetCount)
            {
                FinishRebuild();
            }

            state.Dependency = default;
            return;
        }
    }

    private void FinishRebuild()
    {
        _lastCompletedBoidCount = _rebuildTargetCount;
        _isRebuilding = false;
        _rebuildTargetCount = 0;
        _populateCursor = 0;
    }

    [BurstCompile]
    public partial struct PopulateSpatialHashChunkJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int3, BoidData>.ParallelWriter SpatialMap;
        public NativeParallelHashSet<int3>.ParallelWriter TouchedCells;

        public float CellSize;
        public int StartIndex;
        public int Count;

        void Execute(Entity entity, [EntityIndexInQuery] int index, in LocalTransform transform, in Velocity velocity)
        {
            if (index < StartIndex || index >= StartIndex + Count) return;

            float3 pos = transform.Position;
            int3 cell = SpatialHashUtils.GetSpatialHash(pos, CellSize);

            BoidData data = new BoidData
            {
                Entity = entity,
                Position = pos,
                Velocity = velocity.Value
            };

            SpatialMap.Add(cell, data);
            TouchedCells.Add(cell);
        }
    }
}
