using UnityEngine;

public class MiniBossHealth : MonoBehaviour
{
    public float maxHealth = 300f;
    private float currentHealth;

    private Animator    animator;
    private MiniBossAI  ai;
    private bool        isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        animator      = GetComponent<Animator>();
        ai            = GetComponent<MiniBossAI>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"[MiniBoss] Levou {amount} de dano. HP: {currentHealth}/{maxHealth} | Accumulated: {ai.damageTaken}/{ai.reactionThreshold}");

        if (currentHealth <= 0)
            Die();
        else
            ai.OnHit(amount);
    }

    void Die()
    {
        isDead = true;
        ai.enabled = false;
        animator.ResetTrigger("Reaction");
        animator.SetTrigger("Death");
        GetComponent<Collider>().enabled = false;
        Debug.Log("[MiniBoss] Morreu.");
        Destroy(gameObject, 5f);
    }

    public bool IsDead() => isDead;
}