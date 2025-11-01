using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Time Manager")]
    public TimeSlow timeManager; // Optional time slow manager

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

    [Header("Punch Settings")]
    public float punchRange = 2f;
    public float punchRadius = 0.8f;
    public int punchDamage = 25;
    public AudioClip punchSwingSound;
    public AudioClip punchHitSound;
    public ParticleSystem punchImpactEffect;

    [Header("References")]
    public Camera playerCamera;
    public Animator animator;
    public Transform holdPoint;

    [Header("Held Item")]
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
        HandleTimeSlow();
    }

    #region Movement & Camera
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
    #endregion

    #region Actions: Punch, Throw, Block
    void HandleActions()
    {
        if (animator == null) return;

        // Left click: Punch or Throw
        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            if (heldItem != null)
            {
                string chosenThrow = (Random.value < 0.5f) ? throwTrigger1 : throwTrigger2;
                animator.SetTrigger(chosenThrow);
                // Perform throw immediately (or via animation event)
                heldItem.Throw();
                heldItem = null;
            }
            else
            {
                animator.SetTrigger(punchTrigger);
            }
        }

        // Block
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

    // Animation Event: called during punch animation
    public void PerformPunch()
    {
        // Swing sound
        if (punchSwingSound)
            AudioSource.PlayClipAtPoint(punchSwingSound, playerCamera.transform.position);

        RaycastHit hit;
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        if (Physics.SphereCast(origin, punchRadius, direction, out hit, punchRange))
        {
            Debug.Log("Punched: " + hit.collider.name);

            CustomerBehavior target = hit.collider.GetComponentInParent<CustomerBehavior>();
            if (target != null && target.CanBeHit())
            {
                target.TakeDamage(punchDamage);
                target.RegisterHit();
                target.ApplyHit(hit.point);

                if (punchHitSound)
                    AudioSource.PlayClipAtPoint(punchHitSound, hit.point);

                if (punchImpactEffect)
                    Instantiate(punchImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                if (punchHitSound)
                    AudioSource.PlayClipAtPoint(punchHitSound, hit.point);
            }
        }
    }

    // Animation Event: called at release frame of throw animation
    public void PerformThrow()
    {
        if (heldItem != null)
        {
            heldItem.Throw();
            heldItem = null;
        }
    }
    #endregion

    #region Time Slow
    void HandleTimeSlow()
    {
        if (timeManager == null) return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Time Slow activated!");
            timeManager.DoSlowMotion();
        }
    }
    #endregion
}
