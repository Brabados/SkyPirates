using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class ShipEntityBootstrap : MonoBehaviour
{
    public float radius = 1f;
    private Entity shipEntity;
    private EntityManager entityManager;

    // Store the collider reference for proper disposal
    private BlobAssetReference<Unity.Physics.Collider> colliderReference;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var archetype = entityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(PhysicsCollider),
            typeof(PhysicsMass),
            typeof(PhysicsVelocity),
            typeof(ShipProxyTag)
        );

        shipEntity = entityManager.CreateEntity(archetype);

        Debug.Log($"World.DefaultGameObjectInjectionWorld: {World.DefaultGameObjectInjectionWorld?.Name}");
        Debug.Log($"EntityManager.IsCreated: {World.DefaultGameObjectInjectionWorld?.EntityManager}");

        // Add physics collide
        var geometry = new SphereGeometry
        {
            Center = float3.zero,
            Radius = Mathf.Max(0.1f, radius)
        };

        // Create collider and store reference for disposal
        colliderReference = Unity.Physics.SphereCollider.Create(geometry, OptimizedCollisionLayers.ShipFilter);

        entityManager.SetComponentData(shipEntity, new PhysicsCollider { Value = colliderReference });
        entityManager.SetComponentData(shipEntity, PhysicsMass.CreateKinematic(MassProperties.UnitSphere));
        entityManager.SetComponentData(shipEntity, new PhysicsVelocity());
        entityManager.SetName(shipEntity, "ShipProxy");

        // Set initial transform
        entityManager.SetComponentData(shipEntity, new LocalTransform
        {
            Position = this.transform.position,
            Rotation = this.transform.rotation,
            Scale = 1f
        });

        Debug.Log("Created ECS ship proxy at runtime");
    }

    void Update()
    {
        if (shipEntity != Entity.Null && entityManager.Exists(shipEntity))
        {
            entityManager.SetComponentData(shipEntity, new LocalTransform
            {
                Position = this.transform.position,
                Rotation = this.transform.rotation,
                Scale = 1f
            });
        }
    }

    void OnDestroy()
    {
        // Dispose the collider to prevent memory leak
        if (colliderReference.IsCreated)
        {
            colliderReference.Dispose();
        }

        // Clean up the entity if it still exists
        if (shipEntity != Entity.Null && World.DefaultGameObjectInjectionWorld != null &&
            World.DefaultGameObjectInjectionWorld.IsCreated && entityManager.Exists(shipEntity))
        {
            entityManager.DestroyEntity(shipEntity);
        }
    }

    void OnApplicationQuit()
    {
        // Safety disposal on application quit
        if (colliderReference.IsCreated)
        {
            colliderReference.Dispose();
        }
    }

    // Method to manually clean up resources
    public void CleanupResources()
    {
        if (colliderReference.IsCreated)
        {
            colliderReference.Dispose();
        }

        if (shipEntity != Entity.Null && World.DefaultGameObjectInjectionWorld != null &&
            World.DefaultGameObjectInjectionWorld.IsCreated && entityManager.Exists(shipEntity))
        {
            entityManager.DestroyEntity(shipEntity);
            shipEntity = Entity.Null;
        }
    }
}
