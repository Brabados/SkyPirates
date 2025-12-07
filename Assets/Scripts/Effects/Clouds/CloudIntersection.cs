using System.Collections.Generic;
using UnityEngine;

public class CloudIntersection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem intersectionParticles;
    [SerializeField] private Transform objectToTrack;

    [Header("Detection")]
    [SerializeField] private int surfaceResolution = 16;
    [SerializeField] private float detectionRadius = 1.5f;
    [SerializeField] private float stopThreshold = 0.7f;

    [Header("Emission")]
    [SerializeField] private float particlesPerUnit = 5f;
    [SerializeField] private float emissionInterval = 0.05f;
    [SerializeField] private bool emitOnlyAtEdges = true;
    [SerializeField] private float edgeThickness = 0.3f;
    [SerializeField] private float minParticleSize = 2f;
    [SerializeField] private float maxParticleSize = 5f;

    private Vector3 cloudRadii;
    private float lastEmissionTime;
    private Vector3 previousPosition;
    private Vector3 velocity;
    private Collider trackedCollider;
    private bool isLocalSpace;
    private HashSet<Vector3> previousPoints;

    private void Start()
    {
        if (!ValidateReferences()) return;

        InitializeCloud();
        InitializeTracking();
        ConfigureParticleSystem();

        previousPoints = new HashSet<Vector3>(128);
    }

    private bool ValidateReferences()
    {
        if (intersectionParticles == null || objectToTrack == null)
        {
            enabled = false;
            return false;
        }
        return true;
    }

    private void InitializeCloud()
    {
        cloudRadii = transform.localScale * 0.5f;
    }

    private void InitializeTracking()
    {
        previousPosition = objectToTrack.position;
        trackedCollider = objectToTrack.GetComponent<Collider>();
    }

    private void ConfigureParticleSystem()
    {
        var emission = intersectionParticles.emission;
        emission.enabled = false;
        isLocalSpace = intersectionParticles.main.simulationSpace == ParticleSystemSimulationSpace.Local;
    }

    private void Update()
    {
        UpdateVelocity();

        if (!IsObjectNearCloud() || IsObjectDeepInside())
        {
            previousPoints.Clear();
            return;
        }

        if (Time.time - lastEmissionTime >= emissionInterval)
        {
            EmitAtIntersections();
            lastEmissionTime = Time.time;
        }
    }

    private void UpdateVelocity()
    {
        velocity = (objectToTrack.position - previousPosition) / Time.deltaTime;
        previousPosition = objectToTrack.position;
    }

    private bool IsObjectNearCloud()
    {
        Vector3 toObject = objectToTrack.position - transform.position;
        Vector3 normalizedDist = new Vector3(
            toObject.x / (cloudRadii.x + detectionRadius),
            toObject.y / (cloudRadii.y + detectionRadius),
            toObject.z / (cloudRadii.z + detectionRadius)
        );

        return normalizedDist.sqrMagnitude <= 1.0f;
    }

    private bool IsObjectDeepInside()
    {
        if (trackedCollider == null) return false;

        Bounds bounds = trackedCollider.bounds;
        Vector3[] testPoints = GetBoundingPoints(bounds);

        int insideCount = 0;
        foreach (Vector3 point in testPoints)
        {
            if (IsPointInCloud(point)) insideCount++;
        }

        return (insideCount / (float)testPoints.Length) >= stopThreshold;
    }

    private Vector3[] GetBoundingPoints(Bounds bounds)
    {
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        return new Vector3[9]
        {
            c,
            c + new Vector3(e.x, e.y, e.z),
            c + new Vector3(e.x, e.y, -e.z),
            c + new Vector3(e.x, -e.y, e.z),
            c + new Vector3(e.x, -e.y, -e.z),
            c + new Vector3(-e.x, e.y, e.z),
            c + new Vector3(-e.x, e.y, -e.z),
            c + new Vector3(-e.x, -e.y, e.z),
            c + new Vector3(-e.x, -e.y, -e.z)
        };
    }

    private void EmitAtIntersections()
    {
        List<Vector3> intersectionPoints = FindIntersectionPoints();
        if (intersectionPoints.Count == 0)
        {
            previousPoints.Clear();
            return;
        }

        int particleCount = Mathf.Max(1, Mathf.RoundToInt(particlesPerUnit));

        foreach (Vector3 point in intersectionPoints)
        {
            if (emitOnlyAtEdges && WasPreviouslyIntersecting(point)) continue;
            EmitParticles(point, particleCount);
        }

        UpdatePreviousPoints(intersectionPoints);
    }

    private List<Vector3> FindIntersectionPoints()
    {
        if (trackedCollider == null) return new List<Vector3>();

        List<Vector3> points = new List<Vector3>(64);
        Bounds bounds = trackedCollider.bounds;

        int[] resolution = CalculateResolution(bounds.size);

        if (velocity.sqrMagnitude > 0.01f)
            SampleFacingFaces(points, bounds, resolution);
        else
            SampleAllFaces(points, bounds, resolution);

        return points;
    }

    private int[] CalculateResolution(Vector3 size)
    {
        float magnitude = size.magnitude;
        return new int[3]
        {
            Mathf.Max(4, Mathf.RoundToInt(surfaceResolution * (size.x / magnitude))),
            Mathf.Max(4, Mathf.RoundToInt(surfaceResolution * (size.y / magnitude))),
            Mathf.Max(4, Mathf.RoundToInt(surfaceResolution * (size.z / magnitude)))
        };
    }

    private void SampleFacingFaces(List<Vector3> points, Bounds bounds, int[] res)
    {
        Vector3 dir = velocity.normalized;

        if (Vector3.Dot(dir, Vector3.forward) > 0.3f) SampleFace(points, bounds, Vector3.forward, res[0], res[1]);
        if (Vector3.Dot(dir, Vector3.back) > 0.3f) SampleFace(points, bounds, Vector3.back, res[0], res[1]);
        if (Vector3.Dot(dir, Vector3.right) > 0.3f) SampleFace(points, bounds, Vector3.right, res[1], res[2]);
        if (Vector3.Dot(dir, Vector3.left) > 0.3f) SampleFace(points, bounds, Vector3.left, res[1], res[2]);
        if (Vector3.Dot(dir, Vector3.up) > 0.3f) SampleFace(points, bounds, Vector3.up, res[0], res[2]);
        if (Vector3.Dot(dir, Vector3.down) > 0.3f) SampleFace(points, bounds, Vector3.down, res[0], res[2]);
    }

    private void SampleAllFaces(List<Vector3> points, Bounds bounds, int[] res)
    {
        SampleFace(points, bounds, Vector3.forward, res[0], res[1]);
        SampleFace(points, bounds, Vector3.back, res[0], res[1]);
        SampleFace(points, bounds, Vector3.right, res[1], res[2]);
        SampleFace(points, bounds, Vector3.left, res[1], res[2]);
        SampleFace(points, bounds, Vector3.up, res[0], res[2]);
        SampleFace(points, bounds, Vector3.down, res[0], res[2]);
    }

    private void SampleFace(List<Vector3> points, Bounds bounds, Vector3 normal, int resU, int resV)
    {
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        if (tangent.sqrMagnitude < 0.01f) tangent = Vector3.Cross(normal, Vector3.right);
        tangent.Normalize();

        Vector3 bitangent = Vector3.Cross(normal, tangent);
        Vector3 center = bounds.center + normal * Vector3.Dot(bounds.extents,
            new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z)));

        float invU = 1f / resU;
        float invV = 1f / resV;

        for (int i = 0; i <= resU; i++)
        {
            for (int j = 0; j <= resV; j++)
            {
                Vector3 offset = tangent * ((i * invU - 0.5f) * bounds.size.x) +
                                bitangent * ((j * invV - 0.5f) * bounds.size.y);
                Vector3 point = center + offset;

                if (IsPointInCloud(point)) points.Add(point);
            }
        }
    }

    private bool IsPointInCloud(Vector3 worldPoint)
    {
        Vector3 toPoint = worldPoint - transform.position;
        Vector3 normalizedDist = new Vector3(
            toPoint.x / cloudRadii.x,
            toPoint.y / cloudRadii.y,
            toPoint.z / cloudRadii.z
        );

        return normalizedDist.sqrMagnitude <= 0.25f; // 0.5^2
    }

    private bool WasPreviouslyIntersecting(Vector3 point)
    {
        return previousPoints.Contains(QuantizePoint(point));
    }

    private Vector3 QuantizePoint(Vector3 point)
    {
        float grid = edgeThickness;
        return new Vector3(
            Mathf.Round(point.x / grid) * grid,
            Mathf.Round(point.y / grid) * grid,
            Mathf.Round(point.z / grid) * grid
        );
    }

    private void UpdatePreviousPoints(List<Vector3> current)
    {
        previousPoints.Clear();
        foreach (Vector3 point in current)
        {
            previousPoints.Add(QuantizePoint(point));
        }
    }

    private void EmitParticles(Vector3 position, int count)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = isLocalSpace ? intersectionParticles.transform.InverseTransformPoint(position) : position,
            velocity = CalculateParticleVelocity(position),
            rotation3D = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)),
            startSize = Random.Range(minParticleSize, maxParticleSize),
            startColor = new Color(1f, 1f, 1f, 0.7f),
            startLifetime = Random.Range(1f, 2f)
        };

        intersectionParticles.Emit(emitParams, count);
    }

    private Vector3 CalculateParticleVelocity(Vector3 position)
    {
        if (velocity.sqrMagnitude < 0.01f)
            return (position - transform.position).normalized * Random.Range(1f, 3f);

        Vector3 toCloud = transform.position - position;
        bool entering = Vector3.Dot(velocity.normalized, toCloud.normalized) > 0;

        Vector3 direction = entering ? -velocity.normalized : velocity.normalized;
        direction = AddSpread(direction);

        float speed = Mathf.Lerp(1f, 5f, Mathf.Clamp01(velocity.magnitude * 0.1f));
        return direction * Random.Range(speed * 0.5f, speed);
    }

    private Vector3 AddSpread(Vector3 direction)
    {
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
        if (perpendicular.sqrMagnitude < 0.01f)
            perpendicular = Vector3.Cross(direction, Vector3.right);

        perpendicular.Normalize();
        return (direction + perpendicular * Random.Range(-0.3f, 0.3f)).normalized;
    }
}
