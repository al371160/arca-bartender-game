using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // auto-destroy after some time
    }

    private void OnCollisionEnter(Collision collision)
    {
        CustomerBehavior customer = collision.collider.GetComponent<CustomerBehavior>();
        if (customer != null)
        {
            // Example reaction: bad customer if not already
            customer.BecomeBad();
            Debug.Log($"{customer.name} got hit and became bad!");

            // Optional: apply additional effect here
        }

        // Destroy the projectile on hit
        Destroy(gameObject);
    }
}
