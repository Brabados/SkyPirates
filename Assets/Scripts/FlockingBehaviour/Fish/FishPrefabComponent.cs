using Unity.Entities;
using UnityEngine;


public class FishPrefabAuthoring : MonoBehaviour
{
    public GameObject prefab;

    class Baker : Baker<FishPrefabAuthoring>
    {
        public override void Bake(FishPrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            // This will mark the prefab GameObject for baking and reference it properly
            var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FishPrefabComponent { prefab = prefabEntity });
        }
    }
}

