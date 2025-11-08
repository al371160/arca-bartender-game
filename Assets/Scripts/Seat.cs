using UnityEngine;

public class Seat : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    private CustomerBehavior occupant;

    public void Claim(CustomerBehavior c)
    {
        if (IsOccupied) return; // prevent double claim
        IsOccupied = true;
        occupant = c;
    }

    public void Release()
    {
        IsOccupied = false;
        occupant = null;
    }
}
