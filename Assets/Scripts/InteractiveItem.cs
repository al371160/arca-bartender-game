using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InteractiveItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint; // usually a child of the camera
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Throw Settings")]
    public float throwForce = 15f;
    public float upwardForce = 2f;
    public float spinTorque = 5f;
    public AudioClip hitSound;
    public float hitVolume = 1f;

    [Header("Pour Settings")]
    public float pourAngle = 120f; // how far to tilt before pouring
    public float pourRate = 0.2f;  // liquid drain per second
    public ParticleSystem pourEffect;
    public AudioClip pourSound;
    public float pourVolume = 0.7f;
    public float maxLiquid = 1f;

    private Rigidbody rb;
    private bool isHeld = false;
    private bool isPouring = false;
    private Transform originalParent;
    private AudioSource audioSource;
    private float currentLiquid;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        currentLiquid = maxLiquid;

        if (pourEffect != null)
            pourEffect.Stop();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = pourSound;
        audioSource.volume = pourVolume;
    }

    void Update()
    {
        if (!isHeld)
            TryPickupWithRaycast();
        else
            HandleHeldItem();
    }

    // 🔹 Player looks at item and presses E to pick it up
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

        // Drop
        if (Input.GetKeyDown(pickupKey))
        {
            Drop();
            return;
        }

        // Throw
        if (Input.GetMouseButtonDown(0))
        {
            Throw();
            return;
        }

        // Pour
        HandlePouring();
    }

    private void Pickup()
    {
        isHeld = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.parent = holdPoint;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    private void Drop()
    {
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.parent = originalParent;
        StopPouring();
    }

    private void Throw()
    {
        isHeld = false;
        transform.parent = null;
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply throw forces
        Vector3 throwDir = Camera.main.transform.forward.normalized;
        Vector3 finalForce = throwDir * throwForce + Vector3.up * upwardForce;
        rb.AddForce(finalForce, ForceMode.VelocityChange);
        rb.AddTorque(Camera.main.transform.right * spinTorque, ForceMode.VelocityChange);

        // Prevent self-collision if needed
        Collider projCol = GetComponent<Collider>();
        Collider throwerCol = holdPoint.root.GetComponent<Collider>();
        if (projCol != null && throwerCol != null)
            Physics.IgnoreCollision(projCol, throwerCol);

        StopPouring();
    }

    // 🔹 Handle tilting to pour
    private void HandlePouring()
    {
        float tilt = Vector3.Angle(transform.up, Vector3.up);

        if (tilt > pourAngle && currentLiquid > 0f)
        {
            StartPouring();
            currentLiquid = Mathf.Max(0f, currentLiquid - pourRate * Time.deltaTime);
        }
        else
        {
            StopPouring();
        }
    }

    private void StartPouring()
    {
        if (!isPouring)
        {
            isPouring = true;
            if (pourEffect != null) pourEffect.Play();
            if (audioSource != null && pourSound != null) audioSource.Play();
        }
    }

    private void StopPouring()
    {
        if (isPouring)
        {
            isPouring = false;
            if (pourEffect != null) pourEffect.Stop();
            if (audioSource != null) audioSource.Stop();
        }
    }

    // 🔹 Collision: handle hit sound, damage, ragdoll, etc.
    private void OnCollisionEnter(Collision collision)
    {
        if (hitSound != null)
        {
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            AudioSource.PlayClipAtPoint(hitSound, hitPoint, hitVolume);
        }

        // Optional: interaction with BodyPart/CustomerBehavior
        BodyPart bodyPart = collision.collider.GetComponent<BodyPart>();
        if (bodyPart != null)
        {
            CustomerBehavior customer = bodyPart.GetCustomer();
            if (customer != null)
            {
                var agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.speed = 0f;
                    agent.isStopped = true;
                }

                customer.TakeDamage(10);

                RagdollController ragdoll = customer.GetComponent<RagdollController>();
                if (ragdoll != null && customer.customerIsDead)
                {
                    ragdoll.SetRagdoll(true);

                    // Apply impact force
                    ContactPoint contact = collision.contacts[0];
                    Rigidbody hitRb = contact.otherCollider.attachedRigidbody;
                    if (hitRb != null)
                        hitRb.AddForceAtPosition(collision.relativeVelocity * 5f, contact.point, ForceMode.Impulse);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Camera.main)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Camera.main.transform.position,
                Camera.main.transform.position + Camera.main.transform.forward * pickupRange);
        }
    }
}
