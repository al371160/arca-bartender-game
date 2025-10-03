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
            Debug.Log("Hit customer (trigger): " + customer.name);
        }

        //Destroy(gameObject);
    }
}
