using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // The target object the camera will follow (e.g., the player)
    public Vector3 offset; // The offset from the target's position
    public float smoothSpeed = 0.125f; // How smoothly the camera follows

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCamera: Target not assigned!");
            return;
        }

        // Calculate the desired position based on the target's position and the offset
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Make the camera look at the target
        transform.LookAt(target);
    }
}
