using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Events;

/// <summary>
/// Optimized spline movement system with event-driven architecture
/// </summary>
public class SplineMover : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private int splineIndex = 0;
    [SerializeField] private bool loopSpline = false;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool orientToPath = true;

    [Header("Events")]
    public UnityEvent<SplineMover> OnReachedKnot;
    public UnityEvent<SplineMover> OnReachedSplineEnd;
    public UnityEvent<SplineMover> OnReachedSplineStart;
    public UnityEvent<SplineMover> OnDetourStarted;
    public UnityEvent<SplineMover> OnDetourCompleted;

    // Movement state
    private bool isMoving;
    private float currentDistance;
    private float totalLength;
    private float normalizedProgress;
    private bool movingForward = true;

    // Detour state
    private bool isOnDetour;
    private Vector3 detourStart;
    private Vector3 detourEnd;
    private float detourDistance;
    private float detourTraveled;
    private float returnProgress;
    private bool wasMovingForward;

    // Cached values
    private Transform cachedTransform;
    private Spline cachedSpline;
    private float speedDeltaTime;

    // Public properties
    public bool IsMoving => isMoving;
    public bool IsOnDetour => isOnDetour;
    public float NormalizedProgress => normalizedProgress;
    public bool MovingForward => movingForward;
    public SplineContainer CurrentSpline => splineContainer;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Start()
    {
        if (splineContainer != null)
        {
            cachedSpline = splineContainer.Splines[splineIndex];
            totalLength = cachedSpline.GetLength();
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        speedDeltaTime = speed * Time.deltaTime;

        if (isOnDetour)
            UpdateDetour();
        else
            UpdateMainSpline();
    }

    private void UpdateMainSpline()
    {
        currentDistance += speedDeltaTime * (movingForward ? 1f : -1f);
        normalizedProgress = currentDistance / totalLength;

        // Handle looping or clamping
        if (loopSpline)
        {
            // Wrap around for circular splines
            if (normalizedProgress > 1f)
            {
                normalizedProgress -= Mathf.Floor(normalizedProgress);
                currentDistance = normalizedProgress * totalLength;
            }
            else if (normalizedProgress < 0f)
            {
                normalizedProgress = 1f - (Mathf.Abs(normalizedProgress) - Mathf.Floor(Mathf.Abs(normalizedProgress)));
                currentDistance = normalizedProgress * totalLength;
            }
        }
        else
        {
            // Clamp and stop at ends
            normalizedProgress = Mathf.Clamp01(normalizedProgress);

            if (normalizedProgress >= 1f)
            {
                isMoving = false;
                OnReachedSplineEnd?.Invoke(this);
            }
            else if (normalizedProgress <= 0f)
            {
                isMoving = false;
                OnReachedSplineStart?.Invoke(this);
            }
        }

        UpdateTransformOnSpline();
    }

    private void UpdateTransformOnSpline()
    {
        float3 pos = splineContainer.EvaluatePosition(splineIndex, normalizedProgress);
        cachedTransform.position = pos;

        if (orientToPath)
        {
            float3 tangent = splineContainer.EvaluateTangent(splineIndex, normalizedProgress);
            if (!movingForward) tangent = -tangent;

            if (math.lengthsq(tangent) > 0.001f)
            {
                float3 up = splineContainer.EvaluateUpVector(splineIndex, normalizedProgress);
                cachedTransform.rotation = Quaternion.LookRotation(tangent, up);
            }
        }
    }

    private void UpdateDetour()
    {
        detourTraveled += speedDeltaTime;
        float t = detourTraveled / detourDistance;

        if (t >= 1f)
        {
            cachedTransform.position = detourEnd;
            FinishDetour();
            return;
        }

        cachedTransform.position = Vector3.Lerp(detourStart, detourEnd, t);

        if (orientToPath)
        {
            Vector3 dir = detourEnd - detourStart;
            if (dir.sqrMagnitude > 0.001f)
                cachedTransform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void FinishDetour()
    {
        isOnDetour = false;
        movingForward = wasMovingForward;
        normalizedProgress = returnProgress;
        currentDistance = normalizedProgress * totalLength;

        UpdateTransformOnSpline();
        isMoving = false;

        OnReachedKnot?.Invoke(this);
        OnDetourCompleted?.Invoke(this);

        // Resume after one frame to prevent double-update
        Invoke(nameof(StartMovement), 0.02f);
    }

    /// <summary>
    /// Move to nearest knot on spline, then continue toward target end
    /// </summary>
    public void MoveToNearestKnotThenToEnd(float targetEnd)
    {
        if (splineContainer == null || isOnDetour) return;

        int nearestIndex = FindNearestKnotIndex();
        float nearestT = (float)nearestIndex / (cachedSpline.Count - 1);
        Vector3 targetPos = splineContainer.EvaluatePosition(splineIndex, nearestT);
        float distance = Vector3.Distance(cachedTransform.position, targetPos);

        // Already at knot, start on main spline
        if (distance < 0.5f)
        {
            normalizedProgress = nearestT;
            currentDistance = normalizedProgress * totalLength;
            movingForward = targetEnd >= 0.5f;
            isMoving = true;
            return;
        }

        // Setup detour to knot
        InitializeDetour(nearestT, targetPos, distance, targetEnd >= 0.5f);
    }

    private void InitializeDetour(float targetT, Vector3 targetPos, float distance, bool forward)
    {
        returnProgress = targetT;
        wasMovingForward = forward;

        detourStart = cachedTransform.position;
        detourEnd = targetPos;
        detourDistance = distance;
        detourTraveled = 0f;

        isOnDetour = true;
        isMoving = true;

        OnDetourStarted?.Invoke(this);
    }

    private int FindNearestKnotIndex()
    {
        int nearestIndex = 0;
        float nearestDistSqr = float.MaxValue;

        for (int i = 0; i < cachedSpline.Count; i++)
        {
            Vector3 knotWorldPos = splineContainer.transform.TransformPoint(cachedSpline[i].Position);
            float distSqr = (cachedTransform.position - knotWorldPos).sqrMagnitude;

            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    /// <summary>
    /// Create detour to target position, then return to main spline
    /// </summary>
    public void CreateDetourToTarget(Vector3 targetPosition)
    {
        if (splineContainer == null || isOnDetour) return;

        float distance = Vector3.Distance(cachedTransform.position, targetPosition);

        // Calculate return point slightly ahead on main spline
        float returnT = normalizedProgress + (movingForward ? 0.1f : -0.1f);
        returnT = Mathf.Clamp01(returnT);

        InitializeDetour(returnT, targetPosition, distance, movingForward);
    }

    // Public API
    public void SetSpline(SplineContainer newSpline)
    {
        splineContainer = newSpline;
        if (splineContainer != null)
        {
            cachedSpline = splineContainer.Splines[splineIndex];
            totalLength = cachedSpline.GetLength();
        }
    }

    public void SetSpeed(float newSpeed) => speed = Mathf.Max(0f, newSpeed);
    public float GetSpeed() => speed;
    public void SetLooping(bool loop) => loopSpline = loop;
    public bool GetLooping() => loopSpline;
    public void StartMovement() => isMoving = true;
    public void StopMovement() => isMoving = false;
    public void ReverseDirection() => movingForward = !movingForward;

    public void ResetToStart()
    {
        currentDistance = 0f;
        normalizedProgress = 0f;
        movingForward = true;
        if (splineContainer != null)
            UpdateTransformOnSpline();
    }
}
