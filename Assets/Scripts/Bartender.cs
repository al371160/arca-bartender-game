using UnityEngine;

public class Bartender : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = 100;
    public bool IsDead => currentHealth <= 0;

    public void TakeDamage(int amount) {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"Bartender HP: {currentHealth}/{maxHealth}");
    }
}
