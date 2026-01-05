using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class FlockCenterAuthoring : MonoBehaviour
{
    [Header("Flock Identity")]
    public int FlockID = 0;

    [Header("Spawn Settings")]
    public int SpawnCount = 100;
    public float SpawnRadius = 10f;
    public float InitialSpeed = 2f;

    [Header("Prefab Selection")]
    [Tooltip("Leave empty to allow all registered prefabs")]
    public int[] AllowedPrefabIndices = new int[0];

    [Tooltip("Optional: directly assign specific prefabs (overrides indices)")]
    public GameObject[] SpecificPrefabs = new GameObject[0];

    public class Baker : Baker<FlockCenterAuthoring>
    {
        public override void Bake(FlockCenterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            // Core identity
            AddComponent(entity, new FlockCenterData
            {
                FlockID = authoring.FlockID,
                Position = authoring.transform.position
            });

            AddComponent(entity, new BoidFlockID
            {
                FlockID = authoring.FlockID
            });

            // Per-flock spawn settings (RESTORED, not new)
            AddComponent(entity, new FlockSpawnSettings
            {
                SpawnCount = authoring.SpawnCount,
                SpawnRadius = authoring.SpawnRadius,
                InitialSpeed = authoring.InitialSpeed
            });

            // Prefab selection buffer (UNCHANGED FEATURE)
            var prefabBuffer = AddBuffer<FlockPrefabSelection>(entity);

            if (authoring.SpecificPrefabs.Length > 0)
            {
                foreach (var prefab in authoring.SpecificPrefabs)
                {
                    if (prefab == null) continue;

                    prefabBuffer.Add(new FlockPrefabSelection
                    {
                        PrefabEntity = GetEntity(prefab, TransformUsageFlags.Dynamic),
                        PrefabIndex = -1
                    });
                }
            }
            else if (authoring.AllowedPrefabIndices.Length > 0)
            {
                foreach (int index in authoring.AllowedPrefabIndices)
                {
                    prefabBuffer.Add(new FlockPrefabSelection
                    {
                        PrefabIndex = index,
                        PrefabEntity = Entity.Null
                    });
                }
            }
            else
            {
                // -1 means "use all prefabs"
                prefabBuffer.Add(new FlockPrefabSelection
                {
                    PrefabIndex = -1,
                    PrefabEntity = Entity.Null
                });
            }
        }
    }
}
