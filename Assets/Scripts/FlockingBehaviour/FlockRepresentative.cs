using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// A MonoBehaviour representative that tracks an ECS flock for targeting purposes.
/// This calculates the approximate center of the flock and registers with the targeting system.
/// </summary>
public class FlockRepresentative : MonoBehaviour
{
    [Header("Flock Reference")]
    [Tooltip("The FlockID this representative tracks")]
    public int flockID;

    [Header("Update Settings")]
    [Tooltip("How often to recalculate flock center (in seconds)")]
    public float updateInterval = 0.1f;

    private float updateTimer;
    private EntityManager entityManager;
    private World defaultWorld;
    private int lastBoidCount;

    void Start()
    {
        // Get reference to ECS world
        defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld != null)
        {
            entityManager = defaultWorld.EntityManager;
        }

        // Initial position update
        UpdateFlockCenter();

        // Register with targeting system
        EventManager.TriggerZTargetRegister(transform);
        Debug.Log("Rep Assigned");
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            UpdateFlockCenter();
            updateTimer = 0f;
        }
    }

    void OnDestroy()
    {
        // Unregister from targeting system
        EventManager.TriggerZTargetUnregister(transform);
    }

    /// <summary>
    /// Calculates the approximate center of the flock by averaging boid positions
    /// </summary>
    private void UpdateFlockCenter()
    {
        if (defaultWorld == null || !defaultWorld.IsCreated)
        {
            return;
        }

        // Query for all boids in this flock
        var query = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<BoidFlockID, LocalTransform>()
            .Build(entityManager);

        var entities = query.ToEntityArray(Allocator.Temp);
        var flockIDs = query.ToComponentDataArray<BoidFlockID>(Allocator.Temp);
        var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        // Calculate center position
        Vector3 centerPosition = Vector3.zero;
        int boidCount = 0;

        for (int i = 0; i < entities.Length; i++)
        {
            if (flockIDs[i].FlockID == flockID)
            {
                centerPosition += (Vector3)transforms[i].Position;
                boidCount++;
            }
        }

        if (boidCount > 0)
        {
            centerPosition /= boidCount;
            transform.position = centerPosition;
            lastBoidCount = boidCount;
        }

        // Cleanup
        entities.Dispose();
        flockIDs.Dispose();
        transforms.Dispose();
        query.Dispose();
    }

    /// <summary>
    /// Gets the current number of boids in this flock
    /// </summary>
    public int GetBoidCount()
    {
        return lastBoidCount;
    }

    /// <summary>
    /// Gets the FlockID this representative is tracking
    /// </summary>
    public int GetFlockID()
    {
        return flockID;
    }

}
