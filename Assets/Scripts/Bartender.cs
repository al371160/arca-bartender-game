using UnityEngine;
using UnityEngine.SceneManagement; // for restarting scene
using TMPro; // for TextMeshPro
using CustomerBehavior = CustomerBehavior;

public class Bartender : MonoBehaviour
{

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public bool IsDead => currentHealth <= 0;

    [Header("Score System")]
    public float survivalTime = 0f;
    public TMP_Text scoreText; // TextMeshPro UI text


    private bool isGameOver = false;

    void Update()
    {
        // Count survival time while alive
        if (!IsDead)
        {
            survivalTime += Time.deltaTime;

            if (scoreText != null)
                scoreText.text = $"Survival Time: {survivalTime:F1}s";
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"Bartender HP: {currentHealth}/{maxHealth}");

        if (IsDead)
            Die();
    }

    private void Die()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"💀 Bartender died after surviving {survivalTime:F1} seconds.");

        // Restart after short delay
        StartCoroutine(RestartSceneAfterDelay(2f));
    }

    private System.Collections.IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
