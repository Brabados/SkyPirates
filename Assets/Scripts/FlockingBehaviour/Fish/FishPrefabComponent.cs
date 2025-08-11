using Unity.Entities;
using UnityEngine;

public class FishPrefabRegistryAuthoring : MonoBehaviour
{
    public GameObject[] prefabs;

    class Baker : Baker<FishPrefabRegistryAuthoring>
    {
        public override void Bake(FishPrefabRegistryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<FishPrefabReference>(entity);

            foreach (var go in authoring.prefabs)
            {
                var prefabEntity = GetEntity(go, TransformUsageFlags.Dynamic);
                buffer.Add(new FishPrefabReference { Prefab = prefabEntity });
            }
        }
    }
}
public struct FishPrefabReference : IBufferElementData
{
    public Entity Prefab;
}

