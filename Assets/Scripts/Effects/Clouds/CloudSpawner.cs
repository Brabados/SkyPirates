using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject cloudPrefab;
    [SerializeField] private int numberOfClouds = 40;
    [SerializeField] private float maxSpawnDistance = 300f;
    [SerializeField] private float despawnDistance = 600f;
    [SerializeField] private float respawnDistance = 540f; // 90% of despawn distance
    [SerializeField] private float minHeight = 250f;
    [SerializeField] private float maxHeight = 300f;
    [SerializeField] private Vector2 scaleRangeXZ = new Vector2(40f, 80f);
    [SerializeField] private Vector2 scaleRangeY = new Vector2(10f, 80f);
    [SerializeField] private float respawnSpread = 100f; // Random offset around respawn point
    [SerializeField] private float windSpeed = 5f;

    private List<GameObject> activeClouds = new List<GameObject>();
    private List<Rigidbody> cloudRigidbodies = new List<Rigidbody>();
    private Vector3 windDirection;
    private float despawnDistanceSqr;

    void Start()
    {
        InitializeWind();
        despawnDistanceSqr = despawnDistance * despawnDistance;

        // Pre-allocate list capacity
        activeClouds.Capacity = numberOfClouds;
        cloudRigidbodies.Capacity = numberOfClouds;

        // Generate initial clouds
        for (int i = 0; i < numberOfClouds; i++)
        {
            GameObject cloud = SpawnCloud();
            activeClouds.Add(cloud);

            Rigidbody rb = cloud.GetComponent<Rigidbody>();
            if (rb != null)
            {
                cloudRigidbodies.Add(rb);
            }
        }
    }

    private void InitializeWind()
    {
        float windX = Random.Range(0.1f, 1f);
        float windZ = Random.Range(0.1f, 1f);
        windDirection = new Vector3(windX, 0f, windZ).normalized;
    }

    private GameObject SpawnCloud()
    {
        Vector3 spawnPos = new Vector3(
            player.transform.position.x + Random.Range(-maxSpawnDistance, maxSpawnDistance),
            Random.Range(minHeight, maxHeight),
            player.transform.position.z + Random.Range(-maxSpawnDistance, maxSpawnDistance)
        );

        GameObject cloud = Instantiate(cloudPrefab, spawnPos, Quaternion.identity);

        // Set wind velocity on rigidbody
        Rigidbody rb = cloud.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = windDirection * windSpeed;
        }

        // Randomize scale
        cloud.transform.localScale = new Vector3(
            Random.Range(scaleRangeXZ.x, scaleRangeXZ.y),
            Random.Range(scaleRangeY.x, scaleRangeY.y),
            Random.Range(scaleRangeXZ.x, scaleRangeXZ.y)
        );

        return cloud;
    }

    private void RepositionCloud(GameObject cloud)
    {
        Vector3 playerPos = player.transform.position;

        // Spawn upwind so clouds blow toward player
        Vector3 upwindDirection = -windDirection;

        // Calculate base respawn position at respawnDistance
        Vector3 baseRespawnPos = playerPos + (upwindDirection * respawnDistance);

        // Add random spread perpendicular to wind direction
        Vector3 perpendicular = new Vector3(-upwindDirection.z, 0, upwindDirection.x);
        Vector3 randomOffset = perpendicular * Random.Range(-respawnSpread, respawnSpread);
        randomOffset += upwindDirection * Random.Range(-respawnSpread * 0.5f, respawnSpread * 0.5f);

        // Final position with height
        cloud.transform.position = new Vector3(
            baseRespawnPos.x + randomOffset.x,
            Random.Range(minHeight, maxHeight),
            baseRespawnPos.z + randomOffset.z
        );

        // Randomize scale for variety
        cloud.transform.localScale = new Vector3(
            Random.Range(scaleRangeXZ.x, scaleRangeXZ.y),
            Random.Range(scaleRangeY.x, scaleRangeY.y),
            Random.Range(scaleRangeXZ.x, scaleRangeXZ.y)
        );

        // Reset velocity if it has a rigidbody
        Rigidbody rb = cloud.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = windDirection * windSpeed;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Public method for external wind system to update wind direction
    public void UpdateWindDirection(Vector3 newDirection)
    {
        windDirection = newDirection.normalized;

        // Update velocity for all active clouds
        for (int i = 0; i < cloudRigidbodies.Count; i++)
        {
            if (cloudRigidbodies[i] != null)
            {
                cloudRigidbodies[i].velocity = windDirection * windSpeed;
            }
        }
    }

    void Update()
    {
        Vector3 playerPos = player.transform.position;

        // Check clouds and recycle those that are too far
        for (int i = 0; i < activeClouds.Count; i++)
        {
            GameObject cloud = activeClouds[i];

            // Use squared distance to avoid expensive sqrt calculation
            float sqrDistance = (playerPos - cloud.transform.position).sqrMagnitude;

            if (sqrDistance > despawnDistanceSqr)
            {
                // Recycle: just reposition instead of destroy/instantiate
                RepositionCloud(cloud);
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up all clouds when spawner is destroyed
        foreach (GameObject cloud in activeClouds)
        {
            if (cloud != null)
            {
                Destroy(cloud);
            }
        }
        activeClouds.Clear();
    }
}
