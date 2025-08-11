using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;

[RequireComponent(typeof(Rigidbody))]
public class HelicopterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float strafeSpeed = 7f;
    public float verticalSpeed = 5f;

    [Header("Rotation Settings")]
    public float yawSpeed = 90f;
    public float pitchSpeed = 45f;
    public float rollSmoothing = 2f;
    public float maxRollAngle = 30f;

    private Rigidbody rb;

    // Cached input values
    private Vector2 moveInput;
    private float turnInput;
    private float verticalInput;

    // Reference to your input actions
    private BasicControls inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Pull the inputActions from your EventManager
        inputActions = EventManager.EventInstance.inputActions;

        // Enable the OverWorld action map
        inputActions.OverWorld.Enable();
    }

    void OnDisable()
    {
        inputActions?.OverWorld.Disable();
    }

    void Update()
    {
        // Read values from OverWorld action map
        moveInput = inputActions.OverWorld.MoveShip.ReadValue<Vector2>();
        turnInput = inputActions.OverWorld.TurnShip.ReadValue<float>();

        float heightValue = inputActions.OverWorld.Height.ReadValue<float>();
        if(heightValue > 0)
        {
            verticalInput = 1;
        }
        else if (heightValue < 0)
        {
            verticalInput = -1;
        }
        else
        {
            verticalInput = 0;
        }

        if(inputActions.OverWorld.Spawn.triggered)
        {
            var controller = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FishFlockController>();         
            controller.TriggerSpawn();
        }
         
    }

    void FixedUpdate()
    {
        // Local movement
        Vector3 forward = transform.forward * moveInput.y * moveSpeed;  
        Vector3 strafe = transform.right * moveInput.x * strafeSpeed;   
        Vector3 lift = transform.up * verticalInput * verticalSpeed;

        rb.velocity = forward + strafe + lift;

        // Apply rotation
        float yaw = turnInput * yawSpeed * Time.fixedDeltaTime;
       // float pitch = -turnInput.y * pitchSpeed * Time.fixedDeltaTime;

        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
       // Quaternion pitchRotation = Quaternion.Euler(pitch, 0f, 0f);
        rb.MoveRotation(rb.rotation * yawRotation);

        // Optional roll/bank effect
        float targetRoll = -moveInput.x * maxRollAngle;
        Quaternion rollRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, targetRoll);
        rb.rotation = Quaternion.Slerp(rb.rotation, rollRotation, Time.fixedDeltaTime * rollSmoothing);
    }
}
