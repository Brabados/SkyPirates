using Unity.Entities;

public struct SpawnRequest : IComponentData
{
    public Entity spawnerEntity; // entity that contains the FlockSpawner data
    public int amount;           // how many boids to spawn
}
