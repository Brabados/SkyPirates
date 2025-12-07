using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // The target object the camera will follow (e.g., the player)

    [Header("Camera Settings")]
    public float distance = 5f; // Distance from target
    public float height = 2f; // Height above target
    public float smoothSpeed = 0.125f; // How smoothly the camera follows

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f; // Speed of camera rotation
    public float minVerticalAngle = -30f; // Minimum vertical angle
    public float maxVerticalAngle = 60f; // Maximum vertical angle

    [Header("Input Settings")]
    public float mouseSensitivity = 2f;
    public float controllerSensitivity = 100f;

    private BasicControls inputActions;
    private float currentYaw = 0f; // Horizontal rotation
    private float currentPitch = 20f; // Vertical rotation

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
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCamera: Target not assigned!");
            return;
        }

        HandleMouseRotation();
        HandleControllerRotation();
        UpdateCameraPosition();
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
        // Read rotation input from controller (right stick)
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

    // Optional: Method to set camera rotation directly (useful for transitions)
    public void SetRotation(float yaw, float pitch)
    {
        currentYaw = yaw;
        currentPitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    // Optional: Get current camera direction (useful for character movement)
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0; // Flatten to horizontal plane
        return forward.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0; // Flatten to horizontal plane
        return right.normalized;
    }
}
