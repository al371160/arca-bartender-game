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

    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Sound Settings")]
    public AudioClip hitSound;
    public float hitVolume = 1f;

    private Rigidbody rb;
    private bool isHeld = false;
    private Transform originalParent;
    private HashSet<CustomerBehavior> alreadyHitCustomers = new HashSet<CustomerBehavior>();

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

        if (Input.GetMouseButtonDown(0))
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
    }

    private void Drop()
    {
        alreadyHitCustomers.Clear();
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.parent = originalParent;
    }

    private void Throw()
    {
        alreadyHitCustomers.Clear();
        isHeld = false;
        transform.parent = null;
        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 throwDir = Camera.main.transform.forward.normalized;
        Vector3 finalForce = throwDir * throwForce + Vector3.up * upwardForce;
        rb.AddForce(finalForce, ForceMode.Impulse);
        rb.AddTorque(Camera.main.transform.right * spinTorque, ForceMode.Impulse);
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
        Debug.Log(gameObject.name + " hit " + customer.name);

        // Apply hit force (optional)
        Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        Vector3 force = rb.linearVelocity;
        // customer.ApplyHit(hitPoint, force);

        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, hitPoint, hitVolume);
    }
}
