using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }

    private float movementSpeed;
    public float movementSpeedNormal;
    public float movementSpeedFast;
    public float movementTime;
    public float rotationAmount;
    public float minimumZoom;
    public float maximumZoom;
    public Vector3 zoomAmount;

    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newZoom;

    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        if (cameraTransform != null)
        {
            newZoom = cameraTransform.localPosition;
        }
    }

    //Updates components of the camera controler that canchange from frame to frame, might find somewere else to put, rotate for example and zoom.
    void Update()
    {
        Forward = transform.forward;
        Right = new Vector3(Forward.z, 0, -Forward.x);
        movementSpeed = (newZoom.y >= (maximumZoom - minimumZoom) / 2) ? movementSpeedFast : movementSpeedNormal;
    }

    //Sets position based on input, this allows for diagonal movemnt
    public void Movement(Vector2 move)
    {
        newPosition += transform.forward * move.y * movementSpeed + transform.right * move.x * movementSpeed;
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
    }

    //Rotates camera, as long as a number comes though it should work
    public void Rotation(float rotate)
    {
        newRotation *= Quaternion.Euler(Vector3.up * rotate * rotationAmount);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
    }

    //Need to adjust this to fit the scale but it is working
    public void Zoom(float zoom)
    {
        newZoom += zoom * zoomAmount;
        newZoom.y = Mathf.Clamp(newZoom.y, minimumZoom, maximumZoom);
        newZoom.z = Mathf.Clamp(newZoom.z, -maximumZoom, -minimumZoom);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }
}