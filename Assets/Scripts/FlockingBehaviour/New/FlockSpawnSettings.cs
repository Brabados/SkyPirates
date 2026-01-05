using Unity.Entities;

public struct FlockSpawnSettings : IComponentData
{
    public int SpawnCount;
    public float SpawnRadius;
    public float InitialSpeed;

    public float MaxSpeed;
    public float SearchRadius;
    public float CohesionWeight;
    public float AlignmentWeight;
    public float SeparationWeight;

    public float ObstacleAvoidanceDistance;
    public float ObstacleAvoidanceWeight;

    public float BoundaryRadius;
    public float BoundaryWeight;
}
