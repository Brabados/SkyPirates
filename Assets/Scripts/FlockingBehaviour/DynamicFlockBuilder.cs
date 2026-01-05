using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class DynamicFlockCreator : MonoBehaviour
{
    [Tooltip("Leave at -1 to auto-assign unique FlockID")]
    public int flockID = -1;

    EntityManager _em;
    Entity _flockEntity;
    float3 _lastPosition;

    static int _nextFlockID = 100;

    void Start()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _lastPosition = transform.position;

        if (flockID == -1)
            flockID = _nextFlockID++;

        _flockEntity = _em.CreateEntity(typeof(FlockCenterData), typeof(BoidFlockID));
        _em.SetComponentData(_flockEntity, new FlockCenterData
        {
            FlockID = flockID,
            Position = transform.position
        });
        _em.AddComponent<NeedsBoidSpawn>(_flockEntity);
#if UNITY_EDITOR
        _em.SetName(_flockEntity, $"DynamicFlockCenter_{flockID}");
#endif
    }

    void Update()
    {
        if (!_em.Exists(_flockEntity))
            return;

        float3 pos = transform.position;
        if (math.distance(pos, _lastPosition) > 0.01f)
        {
            _em.SetComponentData(_flockEntity, new FlockCenterData
            {
                FlockID = flockID,
                Position = pos
            });
            _lastPosition = pos;
        }
    }

    void OnDestroy()
    {
        if (_em.Exists(_flockEntity))
            _em.DestroyEntity(_flockEntity);
    }
}
