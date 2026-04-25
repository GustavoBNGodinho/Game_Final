using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private bool isDead = false;
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
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        Debug.Log($"[Player] HP: {currentHealth}/{maxHealth}");
        if (healthBar) healthBar.value = currentHealth;

        if (currentHealth <= 0)
            Die();
        else
            GetComponent<PlayerController>().OnHit(); // só chama OnHit se ainda vivo
    }

    void Die()
    {
        isDead = true;
        Debug.Log("[Player] Game Over.");
        GetComponent<PlayerController>().OnDeath();
    }
}