using UnityEngine;

public class Seat : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public CustomerBehavior CurrentCustomer { get; private set; }

    public void Claim(CustomerBehavior c) {
        IsOccupied = true;
        CurrentCustomer = c;
    }

    public void Release() {
        IsOccupied = false;
        CurrentCustomer = null;
    }
}
