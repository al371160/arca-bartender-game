using UnityEngine;

public class ArmPunchEvent : MonoBehaviour
{
    public FPSController playerController;

    // This is called via AnimationEvent
    public void PerformPunch()
    {
        if (playerController != null)
            playerController.PerformPunch();
    }
}
