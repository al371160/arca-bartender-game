using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class InteractiveItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Throw Settings")]
    public float throwForce = 8f;
    public float upwardForce = 1f;
    public float spinTorque = 2f;
    public float throwCooldown = 0.3f;
    public bool alignWithCamera = true;

    [Header("Throw Angle Settings")]
    [Range(-45f, 45f)] public float horizontalAngle = 0f; // left/right in degrees
    [Range(-45f, 45f)] public float verticalAngle = 0f;   // up/down in degrees

    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Sound Settings")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 0.5f;

    private Rigidbody rb;
    private bool isHeld = false;
    private Transform originalParent;
    private float lastThrowTime = 0f;
    private HashSet<CustomerBehavior> alreadyHitCustomers = new HashSet<CustomerBehavior>();

    private FPSController playerController;
    private Transform currentHoldPoint;
    private bool isLeftHand = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
    }

    void Update()
    {
        if (!isHeld)
            TryPickupWithRaycast();
        else
            HandleHeldItem();
    }

    private void TryPickupWithRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.GetComponent<InteractiveItem>() == this && Input.GetKeyDown(pickupKey))
            {
                bool left = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                PickupToCustomHoldPoint(left);
            }
        }
    }

    private void HandleHeldItem()
    {
        if (currentHoldPoint != null)
        {
            transform.position = currentHoldPoint.position;
            transform.rotation = currentHoldPoint.rotation;
        }

        /* if (Input.GetKeyDown(pickupKey))
        {
            Drop();
            return;
        } */

        /*if (Input.GetMouseButtonDown(0) && Time.time - lastThrowTime > throwCooldown)
        {
            Throw();
        }*/
    }

    // --------- FIXED METHOD: Works with left/right hands ----------
    public void PickupToCustomHoldPoint(bool leftHand)
    {
        alreadyHitCustomers.Clear();
        isHeld = true;
        rb.isKinematic = true;
        rb.useGravity = false;

        if (playerController == null)
            playerController = FindFirstObjectByType<FPSController>();

        if (playerController != null)
        {
            isLeftHand = leftHand;
            currentHoldPoint = leftHand ? playerController.leftHoldPoint : playerController.rightHoldPoint;
            transform.SetParent(currentHoldPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (leftHand)
                playerController.leftHeldItem = this;
            else
                playerController.rightHeldItem = this;

            // 🔹 Trigger pickup animation on the correct arm
            Animator anim = leftHand ? playerController.leftArmAnimator : playerController.rightArmAnimator;
            if (anim != null)
                anim.SetTrigger("Pickup");
        }
    }

    public void Drop()
    {
        alreadyHitCustomers.Clear();
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.parent = originalParent;

        if (playerController != null)
        {
            Animator anim = isLeftHand ? playerController.leftArmAnimator : playerController.rightArmAnimator;
            if (anim != null)
                anim.SetTrigger("Drop");

            if (isLeftHand)
                playerController.leftHeldItem = null;
            else
                playerController.rightHeldItem = null;
        }

        playerController = null;
        currentHoldPoint = null;
    }


    public void Throw()
    {
        alreadyHitCustomers.Clear();
        isHeld = false;
        transform.parent = null;
        rb.isKinematic = false;
        rb.useGravity = true;
        lastThrowTime = Time.time;

        if (playerController != null)
        {
            Animator anim = isLeftHand ? playerController.leftArmAnimator : playerController.rightArmAnimator;
            if (anim != null)
                anim.SetTrigger("Throw");
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        // Base direction
        Vector3 throwDir = cam.transform.forward.normalized;

        // Apply horizontal and vertical rotation
        Quaternion horizontalRot = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
        Quaternion verticalRot = Quaternion.AngleAxis(verticalAngle, cam.transform.right);

        throwDir = verticalRot * horizontalRot * throwDir;

        Vector3 finalForce = throwDir * throwForce + Vector3.up * upwardForce;

        rb.AddForce(finalForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spinTorque, ForceMode.Impulse);
    }


    private void OnCollisionEnter(Collision collision)
    {
        BodyPart bodyPart = collision.collider.GetComponent<BodyPart>();
        if (bodyPart == null || bodyPart.customer == null) return;

        CustomerBehavior customer = bodyPart.customer;
        if (alreadyHitCustomers.Contains(customer)) return;
        alreadyHitCustomers.Add(customer);

        if (!customer.CanBeHit()) return;
        customer.RegisterHit();
        customer.TakeDamage(damage);
        Debug.Log($"{gameObject.name} hit {customer.name}");

        ContactPoint contact = collision.contacts[0];
        Rigidbody hitRb = contact.otherCollider.attachedRigidbody;
        if (hitRb != null)
            hitRb.AddForceAtPosition(collision.relativeVelocity * -8f, contact.point, ForceMode.Impulse);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, contact.point, hitVolume);
    }
}
