using Unity.Entities;
using UnityEngine;

public class FlockSpawnInputHandler : MonoBehaviour
{
    public Transform referenceTransform;

    EntityManager _em;

    void Start()
    {
        if (referenceTransform == null)
            referenceTransform = transform;

        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // replace with your input system
        {
            SpawnFlockCenter(referenceTransform.position);
        }
    }

    void SpawnFlockCenter(Vector3 position)
    {
        var entity = _em.CreateEntity(typeof(FlockCenterData));
        _em.SetComponentData(entity, new FlockCenterData
        {
            FlockID = Random.Range(1000, 9999),
            Position = position
        });

        Debug.Log("Spawned flock center via input");
    }
}
