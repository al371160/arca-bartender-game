using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int starRating = 5;

    public void RegisterRequest(CustomerBehavior c) {
        Debug.Log($"Customer {c.name} requested a drink!");
        // TODO: show UI graphic
    }

    public void ApplyPenalty(int amount, string reason) {
        starRating -= amount;
        Debug.Log($"Penalty: {reason}. Stars: {starRating}");
    }

    public void AddTip(int amount) {
        Debug.Log($"+{amount} tip!");
    }

    public void OnBartenderDied() {
        Debug.Log("Game Over: Bartender choked out!");
        // freeze game, show screen, etc.
    }
}
