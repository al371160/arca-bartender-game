using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Time Manager")]
    public TimeSlow timeManager;

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
    public Animator leftArmAnimator;
    public Animator rightArmAnimator;
    public Transform leftHoldPoint;
    public Transform rightHoldPoint;

    [Header("Crosshair UI")]
    public UnityEngine.UI.Image crosshairImage;
    public Sprite defaultCrosshair;
    public Sprite canPickupCrosshair;

    [Header("Held Items")]
    public InteractiveItem leftHeldItem;
    public InteractiveItem rightHeldItem;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool wasGrounded;
    private bool isBlocking;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleActions();
        HandleTimeSlow();
        UpdateCrosshair();
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
        wasGrounded = controller.isGrounded;
        if (wasGrounded && velocity.y < 0) velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 desiredMove = transform.right * horizontal + transform.forward * vertical;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        if (wasGrounded)
            horizontalVelocity = desiredMove * speed;
        else
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredMove * speed, midAirControl);

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        if (wasGrounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Time Slow Test with Movement
        /*if (desiredMove == Vector3.zero)
        { 
            timeManager.DoSlowMotion();
        }*/
    }
    #endregion

    #region Actions
    void HandleActions()
    {
        bool leftShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // ---- PICKUP / DROP ----
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupOrDrop(leftShift); // shift = left hand
        }

        // ---- LEFT CLICK ----
        if (Input.GetMouseButtonDown(0))
        {
            if (leftShift) // Shift + Click = left hand action
            {
                if (leftHeldItem != null)
                {
                    ThrowItem(leftHeldItem, leftArmAnimator);
                    leftHeldItem = null;
                }
                else if (!isBlocking)
                {
                    PunchRandomHand();
                }
            }
            else // Click without shift = right hand action
            {
                if (rightHeldItem != null)
                {
                    ThrowItem(rightHeldItem, rightArmAnimator);
                    rightHeldItem = null;
                }
                else if (!isBlocking)
                {
                    PunchRandomHand();
                }
            }
        }

        // ---- BLOCK ----
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isBlocking = true;
            SetBothArmsBool(blockBool, true);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            isBlocking = false;
            SetBothArmsBool(blockBool, false);
        }
    }

    // ----------------- PUNCH -----------------
    void PunchRandomHand()
    {
        // Only punch randomly if both hands are empty
        if (leftHeldItem != null && rightHeldItem != null)
            return; // both hands full, no punch

        bool useLeft;

        if (leftHeldItem != null) // left hand occupied, punch right hand
            useLeft = false;
        else if (rightHeldItem != null) // right hand occupied, punch left hand
            useLeft = true;
        else
            useLeft = Random.value < 0.5f; // both hands free, random

        if (useLeft && leftArmAnimator)
            leftArmAnimator.SetTrigger(punchTrigger);
        else if (!useLeft && rightArmAnimator)
            rightArmAnimator.SetTrigger(punchTrigger);

        PerformPunch();
    }

    void UpdateCrosshair()
    {
        if (playerCamera == null || crosshairImage == null) return;

        bool canPickup = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f)) // same pickup range
        {
            if (hit.collider.GetComponent<InteractiveItem>() != null)
                canPickup = true;
        }

        // Change sprite
        crosshairImage.sprite = canPickup ? canPickupCrosshair : defaultCrosshair;

        // Instantly scale
        crosshairImage.rectTransform.localScale = canPickup ? Vector3.one * 2f : Vector3.one;
    }


    void TryPickupOrDrop(bool leftHand)
    {
        Camera cam = playerCamera;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            InteractiveItem hitItem = hit.collider.GetComponent<InteractiveItem>();
            InteractiveItem currentItem = leftHand ? leftHeldItem : rightHeldItem;

            if (hitItem != null)
            {
                // Only drop the currently held item if hitting a new item
                if (currentItem != null)
                    currentItem.Drop();

                // Pick up the new item
                hitItem.PickupToCustomHoldPoint(leftHand);
                if (leftHand) leftHeldItem = hitItem;
                else rightHeldItem = hitItem;
            }
        }
    }


    void ThrowItem(InteractiveItem item, Animator arm)
    {
        if (timeManager) timeManager.ResumeTimeTemporarily(0.4f);

        string trigger = (Random.value < 0.5f) ? throwTrigger1 : throwTrigger2;
        if (arm) arm.SetTrigger(trigger);
        item.Throw();
    }


    void PunchBoth()
    {
        if (leftArmAnimator) leftArmAnimator.SetTrigger(punchTrigger);
        if (rightArmAnimator) rightArmAnimator.SetTrigger(punchTrigger);
        PerformPunch();
    }
    #endregion

    #region Punch Detection
    public void PerformPunch()
    {
        // Resume time briefly
        if (timeManager) timeManager.ResumeTimeTemporarily(0.2f);

        // Play swing sound
        if (punchSwingSound)
            AudioSource.PlayClipAtPoint(punchSwingSound, playerCamera.transform.position);

        // SphereCast for punch
        if (Physics.SphereCast(playerCamera.transform.position, punchRadius, playerCamera.transform.forward, out RaycastHit hit, punchRange))
        {
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
        }
    }


    #endregion

    #region Time Slow
    void HandleTimeSlow()
    {
        if (timeManager == null) return;
        if (Input.GetKeyDown(KeyCode.Z))
            timeManager.DoSlowMotion();
    }
    #endregion

    #region Helpers
    void SetBothArmsBool(string boolName, bool value)
    {
        if (leftArmAnimator) leftArmAnimator.SetBool(boolName, value);
        if (rightArmAnimator) rightArmAnimator.SetBool(boolName, value);
    }
    #endregion
}
