using UnityEngine;
using UnityEngine.UI;

public class CustomerHealthBar : MonoBehaviour
{
    [Header("References")]
    public CustomerBehavior customer;  // drag parent Customer here
    public RagdollController ragdoll;  // check if customer is dead
    public Image healthFill;           // drag the Fill Image (UI -> Image) here
    public Camera mainCamera;          // assign main camera in inspector or auto-find

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main; // auto-assign main camera
    }

    void Update()
    {
        if (customer != null && healthFill != null)
        {
            float fillValue = (float)customer.customerCurrentHealth / customer.customerMaxHealth;
            healthFill.fillAmount = fillValue;
            if (customer.customerCurrentHealth == 0)
            {
                Destroy(gameObject); // removes health bar on death
            }
        }

        // Make sure the health bar always faces the camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
