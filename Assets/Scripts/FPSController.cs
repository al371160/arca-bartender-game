using System.Net;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public TimeSlow timeManager; //Added Time Manager

    [Header("Movement Settings")]
    public float speed = 5.0f;
    [Range(0f, 1f)] public float midAirControl = 0.5f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 2.0f;
    public float maxLookAngle = 90.0f;
    public float minLookAngle = -90.0f;

    [Header("Jump & Gravity Settings")]
    public float jumpForce = 5.0f;
    public float gravity = -9.81f;

    [Header("Animation Settings")]
    public string punchTrigger = "Punch";
    public string throwTrigger1 = "Throw1";
    public string throwTrigger2 = "Throw2";
    public string blockBool = "Blocking";

    [Header("References")]
    public Camera playerCamera;
    public Animator animator;
    public Transform holdPoint; // Assign this to your hand bone or camera child

     public InteractiveItem heldItem; // reference to currently held item

    private CharacterController characterController;
    private float verticalRotation = 0f;
    private Vector3 playerVelocity;
    private bool wasGrounded;
    private bool isBlocking;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (animator == null) animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleActions();
        DoTimeSlow();
        Debug.Log(heldItem);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minLookAngle, maxLookAngle);
        playerCamera.transform.localEulerAngles = Vector3.right * verticalRotation;
    }

    void HandleMovement()
    {
        wasGrounded = characterController.isGrounded;
        if (wasGrounded && playerVelocity.y < 0) playerVelocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 desiredMove = transform.right * horizontal + transform.forward * vertical;

        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0f, playerVelocity.z);

        if (wasGrounded)
            horizontalVelocity = desiredMove * speed;
        else
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredMove * speed, midAirControl);

        playerVelocity.x = horizontalVelocity.x;
        playerVelocity.z = horizontalVelocity.z;

        if (wasGrounded && Input.GetButtonDown("Jump"))
            playerVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);

        if (animator != null)
            animator.SetFloat("Speed", new Vector2(horizontal, vertical).magnitude);
    }

    void HandleActions()
    {
        if (animator == null) return;

        // --- Left click: Punch OR Throw ---
        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            if (heldItem != null)
            {
                string chosenThrow = (Random.value < 0.5f) ? throwTrigger1 : throwTrigger2;
                animator.SetTrigger(chosenThrow);
                // Only throw if holding an item
                heldItem.Throw();
                heldItem = null;
            }
            else
            {
                // Normal punch
                animator.SetTrigger(punchTrigger);
            }
        }


        // --- Block ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isBlocking = true;
            animator.SetBool(blockBool, true);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            isBlocking = false;
            animator.SetBool(blockBool, false);
        }
    }


    // --- Animation Event: Called at release frame of throw animation ---
    public void PerformThrow()
    {
        if (heldItem != null)
        {
            heldItem.Throw();
            heldItem = null; // let go of it
        }
    }

    // --- Time Managing Script ---
    public void DoTimeSlow()
    {
        if (Input.GetKeyDown(KeyCode.Z)) //Z activates TimeSlow
        {
            Debug.Log("Z was pressed!");
            timeManager.DoSlowMotion();    
        }
            
    }

}
