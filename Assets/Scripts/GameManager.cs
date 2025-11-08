using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public int starRating = 5;

    private List<CustomerBehavior> activeRequests = new List<CustomerBehavior>();
    public float maxWaitTime = 15f; // time before customer becomes bad if drink not served

    public void RegisterRequest(CustomerBehavior c)
    {
        Debug.Log($"Customer {c.name} requested a drink!");
        activeRequests.Add(c);
        StartCoroutine(WaitForDrinkTimeout(c));
    }

    private IEnumerator WaitForDrinkTimeout(CustomerBehavior c)
    {
        float startTime = Time.time;
        while (Time.time - startTime < maxWaitTime)
        {
            if (c == null || c.customerIsDead || !c.isGood)
                yield break; // stop if dead or already bad or left
            yield return null;
        }

        // Time ran out — become bad!
        if (c != null && c.isGood)
        {
            Debug.LogWarning($"{c.name} waited too long and became bad!");
            c.BecomeBad();
        }
    }

    public void ApplyPenalty(int amount, string reason)
    {
        starRating -= amount;
        Debug.Log($"Penalty: {reason}. Stars: {starRating}");
    }

    public void AddTip(int amount)
    {
        Debug.Log($"+{amount} tip!");
    }

    public void OnBartenderDied()
    {
        Debug.Log("Game Over: Bartender choked out!");
        // freeze game, show screen, etc.
    }
}
