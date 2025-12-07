using Unity.Entities;
using Unity.Mathematics;

public struct FishControllerSettingsComponent : IComponentData
{
    // Spawn settings
    public int SpawnCount;
    public float SpawnRadius;
    public float InitialSpeed;

    // Boid settings
    public float MaxSpeed;
    public float SearchRadius;
    public float CohesionWeight;
    public float AlignmentWeight;
    public float SeparationWeight;
    public float ObstacleAvoidanceDistance;
    public float ObstacleAvoidanceWeight;

    // Boundary settings
    public float3 BoundaryCenter;
    public float BoundaryRadius;
    public float BoundaryWeight;
}

public class FishControllerSettingsBaker : Baker<FishControllerSettings>
{
    public override void Bake(FishControllerSettings authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new FishControllerSettingsComponent
        {
            SpawnCount = authoring.spawnCount,
            SpawnRadius = authoring.spawnRadius,
            InitialSpeed = authoring.initialSpeed,

            MaxSpeed = authoring.maxSpeed,
            SearchRadius = authoring.searchRadius,
            CohesionWeight = authoring.cohesionWeight,
            AlignmentWeight = authoring.alignmentWeight,
            SeparationWeight = authoring.separationWeight,
            ObstacleAvoidanceDistance = authoring.obstacleAvoidanceDistance,
            ObstacleAvoidanceWeight = authoring.obstacleAvoidanceWeight,

            BoundaryCenter = authoring.boundaryCenter,
            BoundaryRadius = authoring.boundaryRadius,
            BoundaryWeight = authoring.boundaryWeight
        });
    }
}
