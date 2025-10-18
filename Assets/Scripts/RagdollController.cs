using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private Rigidbody[] ragdollBodies;
    [SerializeField] private Collider[] ragdollColliders;

    [Header("Ragdoll Force Settings")]
    public float impactForce = 5f;       // horizontal push
    public float upwardForce = 1f;       // vertical lift

    // Public read-only access to limb rigidbodies
    public Rigidbody[] RagdollBodies => ragdollBodies;

    void Awake()
    {
        // Grab all child rigidbodies and colliders
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // Start with animator active
        SetRagdoll(false);
    }

    /// <summary>
    /// Enables or disables ragdoll mode
    /// </summary>
    /// 
    public void SetRagdoll(bool active, Vector3 sourcePosition = default)
    {
        animator.enabled = !active;

        foreach (var rb in ragdollBodies)
        {
            if (rb.gameObject != this.gameObject) // skip root if desired
                rb.isKinematic = !active;
        }

        if (active && sourcePosition != default)
            ApplyHit(sourcePosition);
    }

    private void ApplyHit(Vector3 sourcePosition)
    {
        // Delay by 1 physics frame
        foreach (var rb in ragdollBodies)
        {
            if (rb.gameObject == this.gameObject) continue;

            // Wake up the rigidbody just in case
            rb.WakeUp();

            // Compute push direction
            Vector3 pushDir = (rb.transform.position - sourcePosition).normalized;
            pushDir.y = Mathf.Clamp(pushDir.y + upwardForce, 0f, 2f);

            rb.AddForce(pushDir * impactForce, ForceMode.Impulse);
        }
    }

}
