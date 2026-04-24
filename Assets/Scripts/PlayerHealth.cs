using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public Slider healthBar; // opcional — arraste uma UI Slider

    void Awake()
    {
        currentHealth = maxHealth;
        if (healthBar) healthBar.maxValue = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.Log($"[Player] HP: {currentHealth}/{maxHealth}");

        if (healthBar) healthBar.value = currentHealth;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("[Player] Game Over.");
        // Futuramente: chamar tela de morte, reiniciar cena etc.
    }
}