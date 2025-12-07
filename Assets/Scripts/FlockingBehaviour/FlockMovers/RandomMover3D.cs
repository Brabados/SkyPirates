using UnityEngine;

public class RandomMover3D : MonoBehaviour
{
    public Transform target;          // The transform to move around
    public float radius = 5f;         // Radius around target to wander
    public float moveSpeed = 3f;      // Movement speed
    public float changeInterval = 2f; // How often to pick a new random point

    private Vector3 destination;
    private float timer;

    void Start()
    {
        if (target != null)
            PickNewDestination();
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
        if (Vector3.Distance(transform.position, destination) < 0.5f || timer > changeInterval)
        {
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
