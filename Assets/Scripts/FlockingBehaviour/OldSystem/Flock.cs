using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [SerializeField]
    private GameObject Prefab;
    [SerializeField]
    private int FlockSize;
    [SerializeField]
    private Vector3 FlockBounds;

    public List<Foid> allUnits;

    [Range(0, 200)]
    [SerializeField]
    private float _CohesionDistance;
    public int CohesionDistance { get { return (int)_CohesionDistance; } }

    [Range(0, 200)]
    [SerializeField]
    private float _AllignmentDistance;
    public int AllignmentDistance { get { return (int)_AllignmentDistance; } }

    [Range(0, 200)]
    [SerializeField]
    private float _AvoidenceDistance;
    public int AvoidenceDistance { get { return (int)_AvoidenceDistance; } }

    [Range(0, 200)]
    [SerializeField]
    private float _BoundsDistance;
    public int BoundsDistance { get { return (int)_BoundsDistance; } }

    [Range(0, 10)]
    [SerializeField]
    private float _CohesionWeight;
    public int CohesionWeight { get { return (int)_CohesionWeight; } }

    [Range(0, 10)]
    [SerializeField]
    private float _AllignmentWeight;
    public int AllignmentWeight { get { return (int)_AllignmentWeight; } }

    [Range(0, 10)]
    [SerializeField]
    private float _AvoidenceWeight;
    public int AvoidenceWeight { get { return (int)_AvoidenceWeight; } }
    [Range(0, 10)]
    [SerializeField]
    private float _BoundsWeight;
    public int BoundsWeight { get { return (int)_BoundsWeight; } }

    [Range(0, 20)]
    [SerializeField]
    private float MinSpeed;
    [Range(0, 20)]
    [SerializeField]
    private float MaxSpeed;

    public void Start()
    {
        generateFlock();
    }

    public void Update()
    {
        foreach (Foid f in allUnits)
        {
            f.moveFoid();
        }
    }

    public void generateFlock()
    {
        allUnits = new List<Foid>(FlockSize);

        for (int x = 0; x < FlockSize; x++)
        {
            Vector3 randomvector = UnityEngine.Random.insideUnitSphere;
            randomvector = new Vector3(randomvector.x * FlockBounds.x, randomvector.y * FlockBounds.y, randomvector.z * FlockBounds.z);
            Vector3 spawnPosition = transform.position + randomvector;
            Quaternion spawnRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            allUnits.Add(Instantiate(Prefab, spawnPosition, spawnRotation).GetComponent<Foid>());
            allUnits[x].assignFlock(this);
            allUnits[x].SetSpeed(UnityEngine.Random.Range(MinSpeed, MaxSpeed));
        }
    }
}
