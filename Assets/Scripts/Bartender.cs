using UnityEngine;
using UnityEngine.SceneManagement; // for restarting scene
using TMPro; // for TextMeshPro
using CustomerBehavior = CustomerBehavior;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    public Volume globalVolume;
    private Vignette vignette;

    void Start()
    {
        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out vignette))
            {
                vignette = globalVolume.profile.Add<Vignette>(true);
                vignette.active = true;
            }
            else
            {
                //Debug.LogError("Global Volume not assigned in the inspector!");
            }
        }
    }

    void Update()
    {
        // Count survival time while alive
        if (!IsDead)
        {
            survivalTime += Time.deltaTime;

            if (currentHealth <= 30)
            {
                // 0.458
                float targetVignette = Mathf.Lerp(0f, 0.458f, 0.5f);
                vignette.intensity.value = 0.458f;
            } else
            {
                float targetVignette = Mathf.Lerp(0f, 0f, 0.5f);
                vignette.intensity.value = 0f;
            }

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
