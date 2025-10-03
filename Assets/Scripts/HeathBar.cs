using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Bartender bartender;   // Drag your Bartender GameObject here
    public Image healthFill;      // Assign in Inspector (UI Image for the fill)

    void Update()
    {
        if (bartender != null && healthFill != null)
        {
            float fillValue = (float)bartender.currentHealth / bartender.maxHealth;
            healthFill.fillAmount = fillValue;
        }
    }
}
