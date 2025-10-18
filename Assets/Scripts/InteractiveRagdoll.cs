using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InteractiveRagdoll : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody[] ragdollBodies;
    public Collider[] ragdollColliders;

    [Header("Settings")]
    [Range(0f, 1f)] public float ragdollWeight = 0f; // 0 = full animation, 1 = full physics
    public float forceMultiplier = 5f;
    public float upwardForce = 1f;

    private Vector3[] initialLocalPositions;
    private Quaternion[] initialLocalRotations;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();

        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        initialLocalPositions = new Vector3[ragdollBodies.Length];
        initialLocalRotations = new Quaternion[ragdollBodies.Length];

        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            initialLocalPositions[i] = ragdollBodies[i].transform.localPosition;
            initialLocalRotations[i] = ragdollBodies[i].transform.localRotation;

            // Always allow physics to simulate
            ragdollBodies[i].isKinematic = false;
        }

        foreach (var col in ragdollColliders)
            col.enabled = true;
    }

    void FixedUpdate()
    {
        // Blend animation into physics
        for (int i = 0; i < ragdollBodies.Length; i++)
        {
            Rigidbody rb = ragdollBodies[i];
            if (rb == this.GetComponent<Rigidbody>()) continue;

            Transform t = rb.transform;

            // Target animation position
            Vector3 targetPos = animator.GetBoneTransform(HumanBodyBones.Hips).position; // default fallback

            if (animator.enabled)
            {
                // If we know specific mapping, we could match ragdoll bones
                targetPos = initialLocalPositions[i] + transform.position;
            }

            // Blend physics with animation
            t.position = Vector3.Lerp(t.position, targetPos, 1f - ragdollWeight);
            t.rotation = Quaternion.Slerp(t.rotation, initialLocalRotations[i], 1f - ragdollWeight);
        }
    }

    public void ApplyHit(Vector3 hitPoint, Vector3 force)
    {
        ragdollWeight = 1f; // go full physics
        foreach (var rb in ragdollBodies)
        {
            if (rb.gameObject == this.gameObject) continue;
            Vector3 dir = (rb.position - hitPoint).normalized + Vector3.up * upwardForce;
            rb.AddForce(dir * force.magnitude * forceMultiplier, ForceMode.Impulse);
        }
    }

    public void SetRagdoll(bool active)
{
        animator.enabled = !active;
    ragdollWeight = active ? 1f : 0f;
}

}
