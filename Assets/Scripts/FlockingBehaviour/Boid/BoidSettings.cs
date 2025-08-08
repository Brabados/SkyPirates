using Unity.Entities;

public struct BoidSettings : IComponentData
{
    public float SearchRadius;
    public float CohesionWeight;
    public float AlignmentWeight;
    public float SeparationWeight;
    public float ObstacleAvoidanceWeight;
    public float ObstacleAvoidanceDistance;
    public float MaxSpeed;
}
