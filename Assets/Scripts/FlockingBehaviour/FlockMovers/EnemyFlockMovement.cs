using UnityEngine;


public class EnemyFlockMovement : MonoBehaviour
{
    public Transform target;          // The transform to move around
    public float radius = 5f;         // Radius around target to wander
    public float moveSpeed = 3f;      // Movement speed
    public float changeInterval = 2f; // How often to pick a new random point
    public DynamicFlockCreator flockCreator;
    public GenType Geneerator;
    [SerializeField]
    public string[] files;

    public EnemyPackSO Enemies;

    Transform mainTarget;
    Transform storedTarget;


    private Vector3 destination;
    private float timer;

    void Start()
    {
        if (target != null)
            PickNewDestination();
        mainTarget = FindObjectOfType<HelicopterController>().transform;
        target = this.transform;
        storedTarget = target;
        flockCreator = this.gameObject.GetComponent<DynamicFlockCreator>();
    }

    void Update()
    {
        if (target == null) return;

        // Move towards current destination
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        // If close enough or time runs out, pick new destination
        timer += Time.deltaTime;

        if (Vector3.Distance(mainTarget.position, transform.position) < 50)
        {
            timer = 0;
            target = mainTarget;
            destination = target.position;
            if (flockCreator.HasBoidsWithinDistance(target.position, 5))
            {
                PawnManager.PawnManagerInstance.MapFiles = files;
                PawnManager.PawnManagerInstance.MapGeneration = Geneerator;
                PawnManager.PawnManagerInstance.Enimies = Enemies;
                SceneLoader.LoadBattleScene();

                Destroy(gameObject);
                Debug.LogWarning("Scene Switch Active");
            }
        }
        else if (Vector3.Distance(transform.position, destination) < 0.5f || timer > changeInterval)
        {
            target = storedTarget;
            PickNewDestination();
            timer = 0f;
        }

    }

    public void PickNewDestination()
    {
        // Pick a random point inside a sphere of radius around target
        Vector3 randomOffset = Random.insideUnitSphere * radius;
        destination = target.position + randomOffset;
    }

}
