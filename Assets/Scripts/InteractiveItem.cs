using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class InteractiveItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Throw Settings")]
    public float throwForce = 8f;
    public float upwardForce = 1f;
    public float spinTorque = 2f;
    public float throwCooldown = 0.3f; // prevents double-throws
    public bool alignWithCamera = true; // makes the item face the throw direction

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

    //pickup/drop animation in fpscontroller
    private FPSController playerController;

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
                Pickup();
            }
        }
    }

    private void HandleHeldItem()
    {
        if (holdPoint != null)
        {
            transform.position = holdPoint.position;
            transform.rotation = holdPoint.rotation;
        }

        if (Input.GetKeyDown(pickupKey))
        {
            Drop();
            return;
        }

        // --- Throw ---
        if (Input.GetMouseButtonDown(0) && Time.time - lastThrowTime > throwCooldown)
        {
            Throw();
        }
    }

    private void Pickup()
    {
        alreadyHitCustomers.Clear();
        isHeld = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.parent = holdPoint;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // --- Directly get FPSController once ---
        if (playerController == null)
            playerController = FindFirstObjectByType<FPSController>();

        if (playerController != null)
        {
            playerController.heldItem = this; // assign directly
            holdPoint = playerController.holdPoint; // use player's hold point
            transform.parent = holdPoint;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
    
    private void Drop()
    {
        alreadyHitCustomers.Clear();
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.parent = originalParent;

        if (playerController != null)
            playerController.heldItem = null;

        playerController = null; // clear reference
    }



    public void Throw()
    {
        alreadyHitCustomers.Clear();
        isHeld = false;
        transform.parent = null;
        rb.isKinematic = false;
        rb.useGravity = true;
        lastThrowTime = Time.time;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Align orientation with camera before throw
        if (alignWithCamera)
            transform.forward = cam.transform.forward;

        // Calculate final throw direction
        Vector3 throwDir = cam.transform.forward.normalized;
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

        // Apply impact force to rigidbody (if exists)
        ContactPoint contact = collision.contacts[0];
        Rigidbody hitRb = contact.otherCollider.attachedRigidbody;
        if (hitRb != null)
            hitRb.AddForceAtPosition(collision.relativeVelocity * -8f, contact.point, ForceMode.Impulse);

        // Play impact sound
        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, contact.point, hitVolume);
    }
}
