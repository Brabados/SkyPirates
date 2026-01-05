using Unity.Entities;
using UnityEngine;

public class FlockSpawnSettingsAuthoring : MonoBehaviour
{
    public int SpawnCount = 50;
    public float SpawnRadius = 5f;
    public float InitialSpeed = 3f;

    public float MaxSpeed = 6f;
    public float SearchRadius = 5f;
    public float CohesionWeight = 1f;
    public float AlignmentWeight = 1f;
    public float SeparationWeight = 1f;

    public float ObstacleAvoidanceDistance = 2f;
    public float ObstacleAvoidanceWeight = 1f;

    public float BoundaryRadius = 25f;
    public float BoundaryWeight = 2f;

    class Baker : Baker<FlockSpawnSettingsAuthoring>
    {
        public override void Bake(FlockSpawnSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new FlockSpawnSettings
            {
                SpawnCount = authoring.SpawnCount,
                SpawnRadius = authoring.SpawnRadius,
                InitialSpeed = authoring.InitialSpeed,

                MaxSpeed = authoring.MaxSpeed,
                SearchRadius = authoring.SearchRadius,
                CohesionWeight = authoring.CohesionWeight,
                AlignmentWeight = authoring.AlignmentWeight,
                SeparationWeight = authoring.SeparationWeight,

                ObstacleAvoidanceDistance = authoring.ObstacleAvoidanceDistance,
                ObstacleAvoidanceWeight = authoring.ObstacleAvoidanceWeight,

                BoundaryRadius = authoring.BoundaryRadius,
                BoundaryWeight = authoring.BoundaryWeight
            });
        }
    }
}
