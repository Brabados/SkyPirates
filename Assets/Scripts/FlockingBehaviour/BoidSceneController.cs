using Unity.Entities;
using UnityEngine;

public class BoidSceneController
{

    public static void HideBoidsAndPause()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        using var allBoids = em.CreateEntityQuery(new EntityQueryDesc
        {
            All = new[] { ComponentType.ReadOnly<BoidTag>() },
            Options = EntityQueryOptions.IncludeDisabledEntities
        });

        em.AddComponent<Disabled>(allBoids);
        Debug.Log("Boids Hidden");
    }

    public static void ShowBoidsAndResume()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        using var pausedBoids = em.CreateEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<BoidTag>(),
                ComponentType.ReadOnly<Disabled>()
            },
            Options = EntityQueryOptions.IncludeDisabledEntities
        });

        em.RemoveComponent<Disabled>(pausedBoids);
    }
}
