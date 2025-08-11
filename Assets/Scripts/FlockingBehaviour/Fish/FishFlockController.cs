using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class FishFlockController : SystemBase
{
    private struct SpawnRequest
    {
        public NativeArray<Entity> Prefabs;
        public FishControllerSettingsComponent Settings;
        public int Remaining;
        public Unity.Mathematics.Random Random;
    }

    private SpawnRequest? _currentSpawn;
    private bool _hasSpawned;

    [SerializeField] private int spawnChunkSize = 250; // fish per frame, tweak for perf

    protected override void OnCreate()
    {
        RequireForUpdate<FishPrefabReference>();
    }

    protected override void OnStartRunning()
    {
        if (!_hasSpawned)
            QueueSpawn();
    }

    public void TriggerSpawn()
    {
        QueueSpawn();
    }

    private void QueueSpawn()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Find the registry entity
        Entity registryEntity = Entity.Null;
        using (var regs = entityManager.CreateEntityQuery(typeof(FishPrefabReference)).ToEntityArray(Allocator.Temp))
        {
            if (regs.Length == 0)
            {
                Debug.LogError("No FishPrefabReference buffer found in subscene!");
                return;
            }
            registryEntity = regs[0];
        }

        // Copy prefabs into safe array
        var prefabBuffer = entityManager.GetBuffer<FishPrefabReference>(registryEntity);
        if (prefabBuffer.Length == 0)
        {
            Debug.LogError("FishPrefabReference buffer is empty!");
            return;
        }

        var prefabEntities = new NativeArray<Entity>(prefabBuffer.Length, Allocator.Persistent);
        for (int i = 0; i < prefabBuffer.Length; i++)
            prefabEntities[i] = prefabBuffer[i].Prefab;

        // Settings
        FishControllerSettingsComponent settings;
        bool hasSettings = SystemAPI.HasSingleton<FishControllerSettingsComponent>();
        if (hasSettings)
        {
            settings = SystemAPI.GetSingleton<FishControllerSettingsComponent>();
        }
        else
        {
            settings = new FishControllerSettingsComponent
            {
                SpawnCount = 1000,
                SpawnRadius = 10f,
                InitialSpeed = 2f,
                MaxSpeed = 5f,
                SearchRadius = 5f,
                CohesionWeight = 1f,
                AlignmentWeight = 1f,
                SeparationWeight = 1f,
                ObstacleAvoidanceDistance = 2f,
                ObstacleAvoidanceWeight = 1f,
                BoundaryCenter = float3.zero,
                BoundaryRadius = 20f,
                BoundaryWeight = 2f
            };
        }

        _currentSpawn = new SpawnRequest
        {
            Prefabs = prefabEntities,
            Settings = settings,
            Remaining = settings.SpawnCount,
            Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue))
        };

        Debug.Log($"Queued spawn of {settings.SpawnCount} fish ({(hasSettings ? "custom" : "default")} settings).");
    }

    protected override void OnUpdate()
    {
        if (!_currentSpawn.HasValue) return;

        var spawn = _currentSpawn.Value;
        int toSpawn = math.min(spawnChunkSize, spawn.Remaining);
        spawn.Remaining -= toSpawn;

        // Choose a random prefab once per fish
        var prefabArray = spawn.Prefabs;
        var prefabCount = prefabArray.Length;
        var rnd = spawn.Random;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var instances = new NativeArray<Entity>(toSpawn, Allocator.TempJob);

        // Batch instantiate
        var prefabChoice = prefabArray[rnd.NextInt(0, prefabCount)];
        entityManager.Instantiate(prefabChoice, instances);

        // Job to set data
        var settings = spawn.Settings;
        var job = new InitFishJob
        {
            Entities = instances,
            InitialSpeed = settings.InitialSpeed,
            MaxSpeed = settings.MaxSpeed,
            SearchRadius = settings.SearchRadius,
            CohesionWeight = settings.CohesionWeight,
            AlignmentWeight = settings.AlignmentWeight,
            SeparationWeight = settings.SeparationWeight,
            ObstacleAvoidanceDistance = settings.ObstacleAvoidanceDistance,
            ObstacleAvoidanceWeight = settings.ObstacleAvoidanceWeight,
            BoundaryCenter = settings.BoundaryCenter,
            BoundaryRadius = settings.BoundaryRadius,
            BoundaryWeight = settings.BoundaryWeight,
            SpawnRadius = settings.SpawnRadius,
            Random = rnd,
            Transforms = GetComponentLookup<LocalTransform>(),
            Velocities = GetComponentLookup<Velocity>(),
            BoidSettingsLookup = GetComponentLookup<BoidSettings>(),
            BoundarySettingsLookup = GetComponentLookup<BoundarySettings>(),
            Ecb = new EntityCommandBuffer(Allocator.TempJob)
        };

        job.Run(toSpawn);

        job.Ecb.Playback(entityManager);
        job.Ecb.Dispose();
        instances.Dispose();

        // Save updated random state
        spawn.Random = job.Random;
        _currentSpawn = spawn;

        if (spawn.Remaining <= 0)
        {
            _currentSpawn.Value.Prefabs.Dispose();
            _currentSpawn = null;
            _hasSpawned = true;
            Debug.Log("Finished spawning all fish.");
        }
    }

    [BurstCompile]
    private struct InitFishJob : IJobParallelFor
    {
        public NativeArray<Entity> Entities;

        public float InitialSpeed;
        public float MaxSpeed;
        public float SearchRadius;
        public float CohesionWeight;
        public float AlignmentWeight;
        public float SeparationWeight;
        public float ObstacleAvoidanceDistance;
        public float ObstacleAvoidanceWeight;
        public float3 BoundaryCenter;
        public float BoundaryRadius;
        public float BoundaryWeight;
        public float SpawnRadius;

        public Unity.Mathematics.Random Random;

        public ComponentLookup<LocalTransform> Transforms;
        public ComponentLookup<Velocity> Velocities;
        public ComponentLookup<BoidSettings> BoidSettingsLookup;
        public ComponentLookup<BoundarySettings> BoundarySettingsLookup;

        public EntityCommandBuffer Ecb;

        public void Execute(int index)
        {
            var entity = Entities[index];
            float3 pos = Random.NextFloat3Direction() * Random.NextFloat(0f, SpawnRadius);
            float3 forward = Random.NextFloat3Direction();

            Ecb.SetComponent(entity, new LocalTransform
            {
                Position = pos,
                Rotation = quaternion.LookRotationSafe(forward, math.up()),
                Scale = 1f
            });

            Ecb.SetComponent(entity, new Velocity { Value = forward * InitialSpeed });

            Ecb.SetComponent(entity, new BoidSettings
            {
                MaxSpeed = MaxSpeed,
                SearchRadius = SearchRadius,
                ObstacleAvoidanceDistance = ObstacleAvoidanceDistance,
                ObstacleAvoidanceWeight = ObstacleAvoidanceWeight,
                CohesionWeight = CohesionWeight,
                AlignmentWeight = AlignmentWeight,
                SeparationWeight = SeparationWeight
            });

            Ecb.SetComponent(entity, new BoundarySettings
            {
                Center = BoundaryCenter,
                Radius = BoundaryRadius,
                BoundaryWeight = BoundaryWeight
            });

            Ecb.AddComponent<BoidTag>(entity);
        }
    }
}
