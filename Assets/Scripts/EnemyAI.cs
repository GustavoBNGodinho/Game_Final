using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Chase, Attack, Hit, Dead }
    public State currentState = State.Idle;

    [Header("Detecção")]
    public float detectionRange = 10f;
    public float attackRange    = 1.8f;

    [Header("Combate")]
    public float attackDamage   = 10f;
    public float attackCooldown = 1.5f;
    public float attackDelay    = 0.5f; // delay até o hit real (no meio da animação)


    [Header("Referências")]
    public Transform player;
    public LayerMask playerLayer;

    private NavMeshAgent agent;
    private Animator animator;
    private PlayerHealth playerHealth; // criaremos na etapa 4

    private float attackTimer    = 0f;

    void Awake()
    {
        agent     = GetComponent<NavMeshAgent>();
        animator  = GetComponent<Animator>();

        // Busca o player automaticamente se não atribuído
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        attackTimer -= Time.deltaTime;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        UpdateState(distToPlayer);
        ExecuteState(distToPlayer);
        UpdateAnimator();
        Debug.Log($"Estado atual: {currentState}");
    }

    void UpdateState(float dist)
    {
        if (dist <= attackRange)
            currentState = State.Attack;
        else if (dist <= detectionRange)
            currentState = State.Chase;
        else
            currentState = State.Idle;
    }

    void ExecuteState(float dist)
    {
        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                break;

            case State.Chase:
                animator.ResetTrigger("Attack");
                agent.isStopped = false;
                agent.SetDestination(player.position);
                agent.stoppingDistance = attackRange;
                break;

            case State.Attack:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

                if (attackTimer <= 0f)
                {
                    attackTimer = attackCooldown;
                    animator.SetTrigger("Attack");
                }
                break;
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    public void ApplyAttackDamage()
    {
        // Só aplica dano se o player ainda está perto
        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            playerHealth?.TakeDamage(attackDamage);
            Debug.Log("[Inimigo] Atacou o player!");
        }
    }

    // Visualização do range no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}