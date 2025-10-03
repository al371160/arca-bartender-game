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

        GameObject proj = Instantiate(projectilePrefab, throwOrigin.position, Quaternion.identity);
        Rigidbody rb = proj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 throwDir = (Camera.main.transform.forward).normalized;
            rb.AddForce(throwDir * throwForce, ForceMode.VelocityChange);
        }
    }
}
