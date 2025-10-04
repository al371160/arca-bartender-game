using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    void Awake()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        SetRagdoll(false); // Start in animator mode
    }

    public void SetRagdoll(bool active)
    {
        animator.enabled = !active;

        foreach (var rb in ragdollBodies)
        {
            if (rb.gameObject != this.gameObject) // skip root
                rb.isKinematic = !active;
        }

        // Leave colliders enabled so projectiles can always hit them
    }

}
