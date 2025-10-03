using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5.0f;
    [Range(0f, 1f)] public float midAirControl = 0.5f; // slider 0=no air control, 1=full control

    [Header("Camera Settings")]
    public float mouseSensitivity = 2.0f;
    public float maxLookAngle = 90.0f;
    public float minLookAngle = -90.0f;

    [Header("Jump & Gravity Settings")]
    public float jumpForce = 5.0f;
    public float gravity = -9.81f;

    [Header("References")]
    public Camera playerCamera;

    private CharacterController characterController;
    private float verticalRotation = 0f;
    private Vector3 playerVelocity;
    private bool wasGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize vertical rotation
        verticalRotation = playerCamera.transform.localEulerAngles.x;
        if (verticalRotation > 180f) verticalRotation -= 360f;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
        playerCamera.transform.localEulerAngles = Vector3.right * verticalRotation;
    }

    void HandleMovement()
    {
        wasGrounded = characterController.isGrounded;

        if (wasGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // keep grounded
        }

        // Input movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 desiredMove = transform.right * horizontal + transform.forward * vertical;

        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);

        if (wasGrounded)
        {
            // Full control on ground
            horizontalVelocity = desiredMove * speed;
        }
        else
        {
            // Airborne: blend current horizontal velocity with input
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredMove * speed, midAirControl);
        }

        // Assign blended horizontal velocity back
        playerVelocity.x = horizontalVelocity.x;
        playerVelocity.z = horizontalVelocity.z;

        // Jumping
        if (wasGrounded && Input.GetButtonDown("Jump"))
        {
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;

        // Move character
        characterController.Move(playerVelocity * Time.deltaTime);
    }

}
