using Unity.Entities;

public static class FishPrefabRegistry
{
    public static Entity[] Prefabs;

    public static void LoadRegistry(EntityManager em)
    {
        using var query = em.CreateEntityQuery(typeof(FishPrefabReference));
        var registryEntity = query.GetSingletonEntity();
        var buffer = em.GetBuffer<FishPrefabReference>(registryEntity);

        Prefabs = new Entity[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
            Prefabs[i] = buffer[i].Prefab;
    }
}
