using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // auto-destroy
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Projectile hit: " + collision.collider.name);
        CustomerBehavior customer = collision.collider.GetComponent<CustomerBehavior>();
        if (customer != null)
        {
                RagdollController ragdoll = collision.collider.GetComponentInParent<RagdollController>();
                if (ragdoll != null)
                {
                    ragdoll.SetRagdoll(true);
                    Debug.Log("hit");

                    // Optional: add impact force
                    Rigidbody hitRb = collision.rigidbody;
                    if (hitRb != null)
                    {
                        hitRb.AddForce(collision.relativeVelocity * 5f, ForceMode.Impulse);
                    }
                }

                //Destroy(gameObject);
        }

        //Destroy(gameObject);
    }
}
