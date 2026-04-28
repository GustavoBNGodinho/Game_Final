using UnityEngine;

public class GoatHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 80f;
    public float currentHealth;
    private bool isDead = false;

    [Header("Referências")]
    private Animator animator;
    private GoatAI goatAI;
    private UnityEngine.AI.NavMeshAgent agent;

    [Header("Nomes das Animações")]
    public string deathAnim = "death"; // Nome da animação no seu pacote Cubic Farm

    void Awake()
    {
        currentHealth = maxHealth-10;
        animator = GetComponent<Animator>();
        goatAI = GetComponent<GoatAI>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"[Cabra] Levou dano! HP: {currentHealth}/{maxHealth}");

        // Você pode adicionar uma animação de "Hit" se o pacote tiver
        // animator.CrossFade("get_hit", 0.1f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[Cabra] Morreu em combate.");

        // Desativa a lógica de IA e movimento
        if (goatAI != null) goatAI.enabled = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Toca a animação de morte usando CrossFade
        animator.CrossFade(deathAnim, 0.2f);

        // Desativa o collider para não atrapalhar o player
        GetComponent<Collider>().enabled = false;

        // Opcional: Destruir após alguns segundos ou deixar o corpo lá
        // Destroy(gameObject, 5f);
    }

    public bool IsDead() => isDead;

    // Método para curar a cabra 
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[Cabra] foi curada! HP: {currentHealth}/{maxHealth}");
    }
}
