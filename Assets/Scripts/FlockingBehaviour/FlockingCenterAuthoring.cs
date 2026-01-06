using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class FlockCenterAuthoring : MonoBehaviour
{
    [Header("Flock Settings")]
    public int FlockID = 0;

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

            AddComponent(entity, new FlockCenterData
            {
                FlockID = authoring.FlockID,
                Position = authoring.transform.position
            });

            AddComponent(entity, new BoidFlockID { FlockID = authoring.FlockID });

            var prefabSelectionBuffer = AddBuffer<FlockPrefabSelection>(entity);

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
            else if (authoring.AllowedPrefabIndices.Length > 0)
            {
                foreach (int index in authoring.AllowedPrefabIndices)
                {
                    prefabSelectionBuffer.Add(new FlockPrefabSelection { PrefabIndex = index, PrefabEntity = Entity.Null });
                }
            }
            else
            {
                // -1 means "use all prefabs"
                prefabSelectionBuffer.Add(new FlockPrefabSelection { PrefabIndex = -1, PrefabEntity = Entity.Null });
            }
        }
    }
}
