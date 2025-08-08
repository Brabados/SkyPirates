using Unity.Physics;

//Optimized collision layers
public static class OptimizedCollisionLayers
{
    public const uint Ship = 1u;
    public const uint Boid = 2u;
    public const uint Default = 4u;

    public static CollisionFilter ShipFilter => new CollisionFilter
    {
        BelongsTo = Ship,
        CollidesWith = Boid | Default,
        GroupIndex = 0
    };

    public static CollisionFilter BoidFilter => new CollisionFilter
    {
        BelongsTo = Boid,
        CollidesWith = Ship | Default,
        GroupIndex = 0
    };

    public static CollisionFilter DefaultFilter => new CollisionFilter
    {
        BelongsTo = Default,
        CollidesWith = Ship | Boid | Default,
        GroupIndex = 0
    };
}
