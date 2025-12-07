using UnityEngine;

[System.Serializable]
public class FishControllerSettings : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int spawnCount = 1000;
    public float spawnRadius = 10f;
    public float initialSpeed = 2f;

    [Header("Boid Behavior Settings")]
    public float maxSpeed = 5f;
    public float searchRadius = 5f;
    public float cohesionWeight = 1f;
    public float alignmentWeight = 1f;
    public float separationWeight = 1f;
    public float obstacleAvoidanceDistance = 2f;
    public float obstacleAvoidanceWeight = 1f;

    [Header("Boundary Settings")]
    public Vector3 boundaryCenter = Vector3.zero;
    public float boundaryRadius = 20f;
    public float boundaryWeight = 2f;
}
