using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;

    [Header("Audio")]
    public AudioClip hitSound;   // assign in Inspector
    public float volume = 1f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // auto-destroy
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile hit: " + collision.collider.name);

        // ✅ Get the body part script
        BodyPart bodyPart = collision.collider.GetComponent<BodyPart>();
        if (bodyPart != null)
        {
            CustomerBehavior customer = bodyPart.GetCustomer();
            if (customer != null)
            {
                // Stop movement (set NavMeshAgent speed to 0)
                var agent = customer.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.speed = 0f;
                    agent.isStopped = true; // guarantee NavMeshAgent halts
                }

                // Apply damage
                customer.TakeDamage((int)damage);

                // Play hit sound at impact point
                if (hitSound != null)
                {
                    Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                    AudioSource.PlayClipAtPoint(hitSound, hitPoint, volume);
                }

                // If dead, trigger ragdoll
                RagdollController ragdoll = customer.GetComponent<RagdollController>();
                if (ragdoll != null && customer.customerIsDead)
                {
                    ragdoll.SetRagdoll(true);
                    Debug.Log("Ragdoll triggered!");

                    // Apply impact force where projectile hit
                    ContactPoint contact = collision.contacts[0];
                    Rigidbody hitRb = contact.otherCollider.attachedRigidbody;
                    if (hitRb != null)
                    {
                        hitRb.AddForceAtPosition(collision.relativeVelocity * 5f, contact.point, ForceMode.Impulse);
                    }
                }
            }
        }

        //Destroy(gameObject); // projectile always destroyed on hit
    }
}
