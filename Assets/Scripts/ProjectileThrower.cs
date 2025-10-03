using UnityEngine;

public class ProjectileThrower : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float throwForce = 15f;
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

        // Compute throw direction
        Vector3 throwDir = Camera.main.transform.forward.normalized;

        // Spawn slightly in front of origin to avoid overlap
        Vector3 spawnPos = throwOrigin.position + throwDir * 0.6f;

        // Align rotation with throw direction
        Quaternion spawnRot = Quaternion.LookRotation(throwDir);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, spawnRot);
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);

            // Ignore collisions with thrower itself
            Collider projCol = proj.GetComponent<Collider>();
            Collider throwerCol = throwOrigin.root.GetComponent<Collider>();
            if (projCol != null && throwerCol != null)
            {
                Physics.IgnoreCollision(projCol, throwerCol);
            }
        }
    }
}
