using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 5f;

    private void Start()
    {
        // Ensure lifetime is valid
        if (lifeTime > 0)
            Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider col)
    {
        CustomerBehavior customer = col.GetComponent<CustomerBehavior>();
        if (customer != null)
        {
            Debug.Log($"{customer.name} got hit!");
        }

        Destroy(gameObject);
    }
}
