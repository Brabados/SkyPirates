using UnityEngine;

public class FlockSpawnInputHandler : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Use player position for closest calculation, or specify a transform")]
    public Transform referenceTransform; // If null, uses this transform
    public BasicControls inputActions;

    // Your input actions reference
    // private InputActions inputActions; // Uncomment and assign your input actions

    void Start()
    {
        if (referenceTransform == null)
            referenceTransform = this.transform;

        inputActions = EventManager.EventInstance.inputActions;
    }

    void Update()
    {
        if (inputActions.OverWorld.Spawn.triggered)
        {
            // Option 1: Use utility (recommended)
            //FlockSpawnUtility.SpawnAtClosest(referenceTransform.position);
        }
    }

}

// Alternative: Static utility class for easy access from anywhere
public static class FlockSpawnUtility
{
    /// <summary>
    /// Spawn fish at the closest flock center to the given world position
    /// </summary>
    public static void SpawnAtClosest(Vector3 worldPosition)
    {
        var controller = FishFlockController.Instance;
        if (controller != null)
        {
            controller.TriggerSpawnAtClosest(worldPosition);
        }
        else
        {
            Debug.LogWarning("FishFlockController not available");
        }
    }

    /// <summary>
    /// Spawn fish at the closest flock center to the main camera position
    /// </summary>
    public static void SpawnAtClosestToCamera()
    {
        if (Camera.main != null)
        {
            SpawnAtClosest(Camera.main.transform.position);
        }
        else
        {
            Debug.LogWarning("Main camera not found");
        }
    }
}
