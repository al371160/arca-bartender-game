using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float speed = 5.0f;
    public float mouseSensitivity = 2.0f;
    public float jumpForce = 5.0f;
    public float gravity = -9.81f;
    public Camera playerCamera;
    public float maxLookAngle = 90.0f;
    public float minLookAngle = -90.0f;
    
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
        // Convert to -180 to 180 range for clamping
        if (verticalRotation > 180f) verticalRotation -= 360f;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        // Mouse look
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
        // Check if grounded at the start of the frame
        wasGrounded = characterController.isGrounded;
        
        // Reset vertical velocity when grounded
        if (wasGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // Small negative value to ensure grounding
        }

        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * speed * Time.deltaTime);

        // Jumping
        if (wasGrounded && Input.GetButtonDown("Jump"))
        {
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Apply gravity
        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
}