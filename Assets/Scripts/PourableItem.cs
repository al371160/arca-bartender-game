using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PourableItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint; // Assign this in inspector (usually under camera)
    public float pickupRange = 3f;
    public KeyCode pickupKey = KeyCode.E;

    [Header("Pour Settings")]
    public float pourAngle = 120f; // How far to tilt before pouring
    public float pourRate = 0.2f;  // Liquid drain per second
    public ParticleSystem pourEffect; // Liquid stream effect
    public AudioClip pourSound;
    public float maxLiquid = 1f;

    [Header("Audio")]
    public float pourVolume = 0.7f;

    private Rigidbody rb;
    private bool isHeld = false;
    private Transform originalParent;
    private AudioSource audioSource;
    private float currentLiquid;
    private bool isPouring;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalParent = transform.parent;
        currentLiquid = maxLiquid;

        if (pourEffect != null)
            pourEffect.Stop();

        // Create or get an AudioSource for pouring
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

    private void TryPickupWithRaycast()
    {
        // Cast a ray from the camera center
        Camera cam = Camera.main;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            PourableItem item = hit.collider.GetComponent<PourableItem>();
            if (item == this && Input.GetKeyDown(pickupKey))
            {
                Pickup();
            }
        }
    }

    private void HandleHeldItem()
    {
        // Follow hand
        if (holdPoint != null)
        {
            transform.position = holdPoint.position;
            transform.rotation = holdPoint.rotation;
        }

        // Drop if pressed again
        if (Input.GetKeyDown(pickupKey))
        {
            Drop();
            return;
        }

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

    private void HandlePouring()
    {
        // Check tilt (bottle forward = pouring direction)
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

        if (currentLiquid <= 0f)
            StopPouring();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(Camera.main ? Camera.main.transform.position : transform.position,
                        (Camera.main ? Camera.main.transform.position : transform.position) + Vector3.forward * pickupRange);
    }
}
