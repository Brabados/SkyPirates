using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

/// <summary>
/// Manages multiple boats on splines, handling spawning and movement coordination
/// </summary>
public class BoatManager : MonoBehaviour
{
    [Header("Boat Settings")]
    [SerializeField] private GameObject boatPrefab;
    [SerializeField] private SplineContainer defaultSpline;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int initialBoatCount = 3;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float defaultBoatSpeed = 5f;
    [SerializeField] private bool autoReverseAtEnds = true;
    [SerializeField] private bool autoStartBoatsToEnd = true;
    [SerializeField] private float autoStartDelay = 0.5f;

    private List<SplineMover> activeBoats = new List<SplineMover>();
    private float lastSpawnTime;

    private void Start()
    {
        // Register any existing boats in the scene
        SplineMover[] existingBoats = FindObjectsOfType<SplineMover>();
        foreach (var boat in existingBoats)
        {
            RegisterBoat(boat);

            // Auto-start existing boats if enabled
            if (autoStartBoatsToEnd)
            {
                StartCoroutine(DelayedStartBoat(boat, autoStartDelay));
            }
        }

        // Spawn initial boats
        for (int i = 0; i < initialBoatCount; i++)
        {
            SpawnBoat();
        }
    }

    private System.Collections.IEnumerator DelayedStartBoat(SplineMover boat, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (boat != null)
        {
            boat.MoveToNearestKnotThenToEnd(1f);
        }
    }

    private void Update()
    {
      
    }

    public SplineMover SpawnBoat()
    {
        if (boatPrefab == null || defaultSpline == null)
        {
            Debug.LogError("Boat prefab or default spline not assigned!");
            return null;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject boatObj = Instantiate(boatPrefab, spawnPos, Quaternion.identity);
        boatObj.name = $"Boat_{activeBoats.Count}";

        SplineMover mover = boatObj.GetComponent<SplineMover>();
        if (mover == null)
        {
            mover = boatObj.AddComponent<SplineMover>();
        }

        // Setup the boat - only set spline, boat keeps its own speed
        mover.SetSpline(defaultSpline);

        // Only set speed if boat doesn't already have a custom speed set
        if (mover.GetSpeed() == 0f || mover.GetSpeed() == 2f) // 2f is default in SplineMover
        {
            mover.SetSpeed(defaultBoatSpeed);
        }

        // Subscribe to events
        mover.OnReachedSplineEnd.AddListener(OnBoatReachedEnd);
        mover.OnReachedSplineStart.AddListener(OnBoatReachedStart);
        mover.OnReachedKnot.AddListener(OnBoatReachedKnot);
        mover.OnDetourStarted.AddListener(OnBoatDetourStarted);
        mover.OnDetourCompleted.AddListener(OnBoatDetourCompleted);

        activeBoats.Add(mover);
        lastSpawnTime = Time.time;

        Debug.Log($"Spawned {boatObj.name} at {spawnPos}");

        // Auto-start the boat if enabled
        if (autoStartBoatsToEnd)
        {
            StartCoroutine(DelayedStartBoat(mover, autoStartDelay));
        }

        EventManager.TriggerZTargetRegister(boatObj.transform);

        return mover;
    }

    public void RegisterBoat(SplineMover boat)
    {
        if (!activeBoats.Contains(boat))
        {
            activeBoats.Add(boat);

            // Subscribe to events
            boat.OnReachedSplineEnd.AddListener(OnBoatReachedEnd);
            boat.OnReachedSplineStart.AddListener(OnBoatReachedStart);
            boat.OnReachedKnot.AddListener(OnBoatReachedKnot);
            boat.OnDetourStarted.AddListener(OnBoatDetourStarted);
            boat.OnDetourCompleted.AddListener(OnBoatDetourCompleted);
            EventManager.TriggerZTargetRegister(boat.gameObject.transform);

            Debug.Log($"Registered existing boat: {boat.gameObject.name}");
        }
    }

    public void SendBoatToNearestKnot(SplineMover boat, bool toEnd)
    {
        if (boat != null)
        {
            boat.MoveToNearestKnotThenToEnd(toEnd ? 1f : 0f);
        }
    }

    public void SendAllBoatsToEnd()
    {
        foreach (var boat in activeBoats)
        {
            if (boat != null && !boat.IsOnDetour)
            {
                boat.MoveToNearestKnotThenToEnd(1f);
            }
        }
    }

    public void SendAllBoatsToStart()
    {
        foreach (var boat in activeBoats)
        {
            if (boat != null && !boat.IsOnDetour)
            {
                boat.MoveToNearestKnotThenToEnd(0f);
            }
        }
    }

    public void StopAllBoats()
    {
        foreach (var boat in activeBoats)
        {
            if (boat != null)
            {
                boat.StopMovement();
            }
        }
    }

    public void StartAllBoats()
    {
        foreach (var boat in activeBoats)
        {
            if (boat != null)
            {
                boat.StartMovement();
            }
        }
    }

    // Event handlers
    private void OnBoatReachedEnd(SplineMover boat)
    {
        Debug.Log($"{boat.gameObject.name} reached END");

        if (autoReverseAtEnds)
        {
            boat.ReverseDirection();
            boat.StartMovement();
        }
    }

    private void OnBoatReachedStart(SplineMover boat)
    {
        Debug.Log($"{boat.gameObject.name} reached START");

        if (autoReverseAtEnds)
        {
            boat.ReverseDirection();
            boat.StartMovement();
        }
    }

    private void OnBoatReachedKnot(SplineMover boat)
    {
        Debug.Log($"{boat.gameObject.name} reached a knot");
    }

    private void OnBoatDetourStarted(SplineMover boat)
    {
        Debug.Log($"{boat.gameObject.name} started detour");
    }

    private void OnBoatDetourCompleted(SplineMover boat)
    {
        Debug.Log($"{boat.gameObject.name} completed detour");
    }

    public List<SplineMover> GetAllBoats()
    {
        return new List<SplineMover>(activeBoats);
    }

    public void RemoveBoat(SplineMover boat)
    {
        if (activeBoats.Contains(boat))
        {
            activeBoats.Remove(boat);

            // Unsubscribe from events
            boat.OnReachedSplineEnd.RemoveListener(OnBoatReachedEnd);
            boat.OnReachedSplineStart.RemoveListener(OnBoatReachedStart);
            boat.OnReachedKnot.RemoveListener(OnBoatReachedKnot);
            boat.OnDetourStarted.RemoveListener(OnBoatDetourStarted);
            boat.OnDetourCompleted.RemoveListener(OnBoatDetourCompleted);
        }
    }
}
