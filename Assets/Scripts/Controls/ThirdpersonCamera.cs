using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The target object the camera will follow (e.g., the player)

    [Header("Camera Settings")]
    public float distance = 10f; // Distance from target
    public float height = 2f; // Height above target
    public float smoothSpeed = 0.125f; // How smoothly the camera follows

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // Speed of camera rotation
    public float minVerticalAngle = -30f; // Minimum vertical angle
    public float maxVerticalAngle = 60f; // Maximum vertical angle

    [Header("Input Settings")]
    public float mouseSensitivity = 2f;
    public float controllerSensitivity = 100f;

    [Header("Z-Targeting Settings")]
    public float lockOnRange = 20f; // Maximum distance to lock onto targets
    public float lockOnBreakDistance = 25f; // Distance at which lock-on breaks
    public float minCameraDistance = 5f; // Minimum camera distance when locked
    public float maxCameraDistance = 15f; // Maximum camera distance when locked
    public float cameraDistancePadding = 2f; // Extra padding to keep both in frame
    public LayerMask obstacleLayer; // Layers that block line of sight

    private BasicControls inputActions;
    private bool cameraControlEnabled = true;
    private float currentYaw = 0f; // Horizontal rotation
    private float currentPitch = 20f; // Vertical rotation

    // Z-Targeting variables
    private bool isLockedOn = false;
    private Transform currentZTarget = null;

    void Start()
    {
        inputActions = EventManager.EventInstance.inputActions;

        // Initialize rotation based on current camera position
        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            currentYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentPitch = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
        }

        // Subscribe to Z-targeting input actions
        inputActions.OverWorld.Ztarget.performed += ctx => ToggleZTarget();
        inputActions.OverWorld.NextTarget.performed += ctx => SwitchTargetRight();
        inputActions.OverWorld.PreciousTarget.performed += ctx => SwitchTargetLeft();
    }

    void OnDestroy()
    {
        // Unsubscribe from input actions
        if (inputActions != null)
        {
            inputActions.OverWorld.Ztarget.performed -= ctx => ToggleZTarget();
            inputActions.OverWorld.NextTarget.performed -= ctx => SwitchTargetRight();
            inputActions.OverWorld.PreciousTarget.performed -= ctx => SwitchTargetLeft();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCamera: Target not assigned!");
            return;
        }

        if (cameraControlEnabled)
        {
            if (isLockedOn)
            {
                CheckLockOnConditions();
                UpdateLockedCamera();
            }
            else
            {
                HandleMouseRotation();
                HandleControllerRotation();
                UpdateCameraPosition();
            }
        }
    }

    private void HandleMouseRotation()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (mouseDelta.magnitude > 0.1f) // Only rotate if mouse is moving
        {
            currentYaw += mouseDelta.x * mouseSensitivity;
            currentPitch -= mouseDelta.y * mouseSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    private void HandleControllerRotation()
    {
        // Read rotation input from controller 
        Vector2 controllerInput = inputActions.Battle.RotateCamera.ReadValue<Vector2>();

        if (controllerInput.magnitude > 0.1f) // Deadzone
        {
            currentYaw += controllerInput.x * controllerSensitivity * Time.deltaTime;
            currentPitch -= controllerInput.y * controllerSensitivity * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        }
    }

    private void UpdateCameraPosition()
    {
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Calculate desired position based on rotation and distance
        Vector3 offset = rotation * new Vector3(0, height, -distance);
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move camera to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Make camera look at target (slightly above center for better view)
        Vector3 lookTarget = target.position + Vector3.up * height * 0.5f;
        transform.LookAt(lookTarget);
    }

    #region Z-Targeting

    public void ToggleZTarget()
    {
        if (isLockedOn)
        {
            UnlockTarget();
        }
        else
        {
            LockOntoNearestTarget();
        }
    }

    private void LockOntoNearestTarget()
    {
        // Get targetables from TargetManager
        List<Transform> targetables = TargetManager.Instance.GetZTargetables();

        if (targetables == null || targetables.Count == 0)
        {
            return;
        }

        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Transform targetable in targetables)
        {
            if (targetable == null) continue;

            float dist = Vector3.Distance(target.position, targetable.position);

            if (dist <= lockOnRange && dist < closestDistance)
            {
                // Check if target is visible (not behind obstacles)
                if (IsTargetVisible(targetable))
                {
                    closest = targetable;
                    closestDistance = dist;
                }
            }
        }

        if (closest != null)
        {
            currentZTarget = closest;
            isLockedOn = true;
        }
    }

    public void UnlockTarget()
    {
        isLockedOn = false;
        currentZTarget = null;
    }

    public void SwitchTargetRight()
    {
        if (!isLockedOn) return;
        SwitchTarget(1); // 1 for right
    }

    public void SwitchTargetLeft()
    {
        if (!isLockedOn) return;
        SwitchTarget(-1); // -1 for left
    }

    private void SwitchTarget(int direction)
    {
        // Get targetables from TargetManager
        List<Transform> targetables = TargetManager.Instance.GetZTargetables();

        if (targetables == null || targetables.Count == 0)
        {
            UnlockTarget();
            return;
        }

        // Filter valid targets within range and visible
        List<Transform> validTargets = targetables
            .Where(t => t != null &&
                   t != currentZTarget &&
                   Vector3.Distance(target.position, t.position) <= lockOnRange &&
                   IsTargetVisible(t))
            .ToList();

        if (validTargets.Count == 0) return;

        // Get current target's screen position
        Vector3 currentScreenPos = Camera.main.WorldToScreenPoint(currentZTarget.position);

        // Find targets to the left or right based on screen position
        Transform newTarget = null;
        float bestScore = float.MaxValue;

        foreach (Transform t in validTargets)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(t.position);
            float horizontalDiff = screenPos.x - currentScreenPos.x;

            // Check if target is in the correct direction
            if ((direction > 0 && horizontalDiff > 0) || (direction < 0 && horizontalDiff < 0))
            {
                // Score based on horizontal distance (prefer closer targets in that direction)
                float score = Mathf.Abs(horizontalDiff);

                if (score < bestScore)
                {
                    bestScore = score;
                    newTarget = t;
                }
            }
        }

        if (newTarget != null)
        {
            currentZTarget = newTarget;
        }
    }

    private void CheckLockOnConditions()
    {
        if (currentZTarget == null)
        {
            UnlockTarget();
            return;
        }

        // Check distance
        float dist = Vector3.Distance(target.position, currentZTarget.position);
        if (dist > lockOnBreakDistance)
        {
            UnlockTarget();
            return;
        }

        // Check visibility
        if (!IsTargetVisible(currentZTarget))
        {
            UnlockTarget();
            return;
        }
    }

    private bool IsTargetVisible(Transform targetTransform)
    {
        if (targetTransform == null) return false;

        // Raycast from player to target
        Vector3 playerToTarget = targetTransform.position - target.position;
        if (Physics.Raycast(target.position + Vector3.up, playerToTarget.normalized, playerToTarget.magnitude, obstacleLayer))
        {
            return false;
        }

        // Raycast from camera to target
        Vector3 cameraToTarget = targetTransform.position - transform.position;
        if (Physics.Raycast(transform.position, cameraToTarget.normalized, cameraToTarget.magnitude, obstacleLayer))
        {
            return false;
        }

        return true;
    }

    private void UpdateLockedCamera()
    {
        if (currentZTarget == null)
        {
            UnlockTarget();
            return;
        }

        // Calculate direction from target to z-target
        Vector3 targetToEnemy = currentZTarget.position - target.position;
        targetToEnemy.y = 0; // Keep on horizontal plane

        if (targetToEnemy.magnitude < 0.1f) return; // Avoid division by zero

        // Camera should be opposite of the enemy relative to player
        Vector3 cameraDirection = -targetToEnemy.normalized;

        // Calculate dynamic distance to keep both in frame
        float distanceBetween = Vector3.Distance(target.position, currentZTarget.position);
        float dynamicDistance = Mathf.Clamp(
            distanceBetween * 0.5f + cameraDistancePadding,
            minCameraDistance,
            maxCameraDistance
        );

        // Position camera behind player, opposite to enemy
        Vector3 desiredPosition = target.position + cameraDirection * dynamicDistance + Vector3.up * height;

        // Smoothly move to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Look at a point between player and enemy (weighted toward enemy)
        Vector3 lookAtPoint = Vector3.Lerp(target.position, currentZTarget.position, 0.6f) + Vector3.up * height * 0.5f;
        transform.LookAt(lookAtPoint);

        // Update yaw and pitch for smooth transition back to free camera
        Vector3 direction = transform.position - target.position;
        currentYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        currentPitch = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
    }

    #endregion

    public void SetRotation(float yaw, float pitch)
    {
        currentYaw = yaw;
        currentPitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }

    public void SetCameraRotationEnabled(bool Setter)
    {
        cameraControlEnabled = Setter;
        return;
    }

    // Public getters for Z-targeting state
    public bool IsLockedOn => isLockedOn;
    public Transform CurrentZTarget => currentZTarget;
}
