using UnityEngine;

public class ProjectileThrower : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float throwForce = 15f;
    public float upwardForce = 2f;   // tweak for arc
    public float spinTorque = 5f;    // tweak for spin
    public Transform throwOrigin;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            Throw();
        }
    }

    private void Throw()
    {
        if (projectilePrefab == null || throwOrigin == null) return;

        Vector3 throwDir = Camera.main.transform.forward.normalized;
        Vector3 spawnPos = throwOrigin.position + throwDir * 0.6f;
        Quaternion spawnRot = Quaternion.LookRotation(throwDir);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, spawnRot);
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Apply force with Impulse
            Vector3 finalForce = throwDir * throwForce + Vector3.up * upwardForce;
            rb.AddForce(finalForce, ForceMode.Impulse);

            // Apply spin with Impulse
            rb.AddTorque(Camera.main.transform.right * spinTorque, ForceMode.Impulse);

            // Ignore self-collision
            Collider projCol = proj.GetComponent<Collider>();
            Collider throwerCol = throwOrigin.root.GetComponent<Collider>();
            if (projCol != null && throwerCol != null)
                Physics.IgnoreCollision(projCol, throwerCol);
        }
    }

}
