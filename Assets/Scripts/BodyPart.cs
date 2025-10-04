using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public CustomerBehavior customer;

    void Awake()
    {
        
    }

    public CustomerBehavior GetCustomer()
    {
        return customer;
    }
}
