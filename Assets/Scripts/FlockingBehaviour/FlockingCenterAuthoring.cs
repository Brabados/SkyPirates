using Unity.Entities;
using UnityEngine;

// Enhanced FlockCenterAuthoring with prefab selection
[DisallowMultipleComponent]
public class FlockCenterAuthoring : MonoBehaviour
{
    [Header("Flock Settings")]
    public int FlockID = 0;

    [Header("Prefab Selection")]
    [Tooltip("Leave empty to use all prefabs, or specify indices of prefabs to use")]
    public int[] AllowedPrefabIndices = new int[0]; // Empty = use all prefabs

    [Header("Optional: Direct Prefab References")]
    [Tooltip("Alternative: directly assign specific prefabs (overrides indices if set)")]
    public GameObject[] SpecificPrefabs = new GameObject[0];

    public class Baker : Baker<FlockCenterAuthoring>
    {
        public override void Bake(FlockCenterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new FlockCenterData
            {
                FlockID = authoring.FlockID,
                Position = authoring.transform.position
            });

            AddComponent(entity, new BoidFlockID { FlockID = authoring.FlockID });

            // Add prefab selection data
            var prefabSelectionBuffer = AddBuffer<FlockPrefabSelection>(entity);

            // If specific prefabs are assigned, use those
            if (authoring.SpecificPrefabs.Length > 0)
            {
                foreach (var prefab in authoring.SpecificPrefabs)
                {
                    if (prefab != null)
                    {
                        var prefabEntity = GetEntity(prefab, TransformUsageFlags.Dynamic);
                        prefabSelectionBuffer.Add(new FlockPrefabSelection { PrefabEntity = prefabEntity });
                    }
                }
            }
            // Otherwise use indices from the registry
            else if (authoring.AllowedPrefabIndices.Length > 0)
            {
                foreach (int index in authoring.AllowedPrefabIndices)
                {
                    prefabSelectionBuffer.Add(new FlockPrefabSelection { PrefabIndex = index, PrefabEntity = Entity.Null });
                }
            }
            // If nothing specified, mark as "use all" with index -1
            else
            {
                prefabSelectionBuffer.Add(new FlockPrefabSelection { PrefabIndex = -1, PrefabEntity = Entity.Null });
            }
        }
    }
}

