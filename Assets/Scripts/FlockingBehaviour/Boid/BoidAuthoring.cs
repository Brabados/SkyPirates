
using UnityEngine;

public class BoidAuthoring : MonoBehaviour
{
    [Header("Boid Settings")]
    public float MaxSpeed = 5f;
    public float SearchRadius = 5f;
    public float CohesionWeight = 1f;
    public float AlignmentWeight = 1f;
    public float SeparationWeight = 1f;
    public float ObstacleAvoidanceDistance = 2f;
    public float ObstacleAvoidanceWeight = 1f;

    [Header("Boundary Settings")]
    public Vector3 BoundaryCenter = Vector3.zero;
    public float BoundaryRadius = 20f;
    public float BoundaryWeight = 2f;
}
