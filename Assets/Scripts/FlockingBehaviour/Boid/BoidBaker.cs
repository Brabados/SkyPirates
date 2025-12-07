using Unity.Entities;
using Unity.Mathematics;

public class BoidAuthoringBaker : Baker<BoidAuthoring>
{
    public override void Bake(BoidAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<Prefab>(entity);
        AddComponent(entity, new Velocity { Value = float3.zero });
        AddComponent(entity, new BoidSettings
        {
            MaxSpeed = authoring.MaxSpeed,
            SearchRadius = authoring.SearchRadius,
            CohesionWeight = authoring.CohesionWeight,
            AlignmentWeight = authoring.AlignmentWeight,
            SeparationWeight = authoring.SeparationWeight,
            ObstacleAvoidanceDistance = authoring.ObstacleAvoidanceDistance,
            ObstacleAvoidanceWeight = authoring.ObstacleAvoidanceWeight
        });

        AddComponent(entity, new BoundarySettings
        {
            Center = authoring.BoundaryCenter,
            Radius = authoring.BoundaryRadius,
            BoundaryWeight = authoring.BoundaryWeight
        });

        AddComponent<BoidTag>(entity);
    }
}
