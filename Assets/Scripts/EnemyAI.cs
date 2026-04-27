using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Investigate, Attack, Dead }
    public State currentState = State.Idle;

    [Header("Patrulha")]
    public float patrolRadius   = 10f;
    public float patrolWaitTime = 2f;
    private float   patrolTimer;
    private Vector3 patrolTarget;

    [Header("Detecção — Cone de Visão")]
    public float visionRange = 15f;
    public float visionAngle = 90f;  // meio ângulo — 90 = cone de 180° total
    public LayerMask obstacleMask;   // atribuir no Inspector: paredes, obstáculos

    [Header("Detecção — Proximidade")]
    public float proximityRange = 2.5f; // sente o player em volta sem precisar ver

    [Header("Combate")]
    public float attackRange    = 1.8f;
    public float attackDamage   = 10f;
    public float attackCooldown = 1.5f;

    [Header("Velocidade")]
    public float patrolSpeed     = 2f;
    public float chaseSpeed      = 5f;
    public float investigateSpeed = 3f;

    [Header("Perseguição")]
    public float pathUpdateInterval = 0.15f; // recalcula a cada 150ms
    private float pathUpdateTimer = 0f;

    [Header("Investigação")]
    public float investigateWaitTime = 4f; // tempo parado no último lugar visto
    private Vector3 lastKnownPosition;
    private float   investigateTimer;

    [Header("Referências")]
    public Transform player;

    private NavMeshAgent agent;
    private Animator     animator;
    private PlayerHealth playerHealth;

    private float attackTimer = 0f;
    private bool  isAttacking = false;
    private bool  isHit       = false;

    // atualização de path — evita recalcular todo frame
    private Vector3 lastPlayerPosition;
    public  float   pathUpdateDistance = 0.5f;

    void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        attackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        UpdateState(dist);
        ExecuteState(dist);
        UpdateAnimator();
    }

    // ─── Detecção ────────────────────────────────────────────────────────────

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float   angle       = Vector3.Angle(transform.forward, dirToPlayer);
        float   dist        = Vector3.Distance(transform.position, player.position);

        if (dist > visionRange) return false;
        if (angle > visionAngle) return false;

        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 target = player.position    + Vector3.up * 1f;

        // Raycast só contra obstáculos — se bater em algo, visão bloqueada
        if (Physics.Raycast(origin, (target - origin).normalized, dist, obstacleMask))
            return false;

        return true;
    }

    bool CanSensePlayer()
    {
        // Proximidade em 360° — independe de ângulo ou linha de visão
        return Vector3.Distance(transform.position, player.position) <= proximityRange;
    }

    bool PlayerDetected() => CanSeePlayer() || CanSensePlayer();

    // ─── State Machine ────────────────────────────────────────────────────────

    void UpdateState(float dist)
    {
        if (isAttacking || isHit) return;

        if (dist <= attackRange && PlayerDetected())
        {
            currentState = State.Attack;
            return;
        }

        if (PlayerDetected())
        {
            lastKnownPosition = player.position;
            // Reseta o timer ao entrar em Chase para atualizar o path imediatamente
            if (currentState != State.Chase)
                pathUpdateTimer = 0f;
            currentState = State.Chase;
            return;
        }

        if (currentState == State.Chase)
        {
            investigateTimer = investigateWaitTime;
            currentState     = State.Investigate;
            return;
        }

        if (currentState != State.Investigate)
            currentState = State.Patrol;
    }

    void ExecuteState(float dist)
    {
        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                break;

            case State.Patrol:
                if (isHit) break;
                agent.isStopped = false;
                agent.speed     = patrolSpeed;
                patrolTimer    -= Time.deltaTime;
                if (patrolTimer <= 0f || agent.remainingDistance < 0.5f)
                {
                    patrolTarget = GetRandomPatrolPoint();
                    agent.SetDestination(patrolTarget);
                    patrolTimer  = patrolWaitTime;
                }
                break;

            case State.Chase:
                if (isHit) break;
                animator.ResetTrigger("Attack");
                agent.isStopped      = false;
                agent.speed          = chaseSpeed;
                agent.stoppingDistance = attackRange;

                pathUpdateTimer -= Time.deltaTime;
                if (pathUpdateTimer <= 0f)
                {
                    agent.SetDestination(player.position);
                    pathUpdateTimer = pathUpdateInterval;
                }
                break;

            case State.Investigate:
                if (isHit) break;
                agent.isStopped      = false;
                agent.speed          = investigateSpeed;
                agent.stoppingDistance = 0.5f;
                agent.SetDestination(lastKnownPosition);

                // Chegou no último lugar visto — espera e volta a patrulhar
                if (agent.remainingDistance < 0.6f)
                {
                    agent.isStopped  = true;
                    investigateTimer -= Time.deltaTime;
                    if (investigateTimer <= 0f)
                        currentState = State.Patrol;
                }
                break;

            case State.Attack:
                agent.isStopped  = true;
                agent.velocity   = Vector3.zero;

                if (isHit) break;

                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

                if (attackTimer <= 0f && !isAttacking)
                {
                    isAttacking  = true;
                    attackTimer  = attackCooldown;
                    animator.ResetTrigger("Attack");
                    animator.SetTrigger("Attack");
                    Invoke(nameof(ResetAttacking), attackCooldown);
                }
                break;
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────

    void ResetAttacking() => isAttacking = false;

    public void OnHit()
    {
        isHit       = true;
        isAttacking = false;
        currentState = State.Idle;
        agent.isStopped = true;
        agent.ResetPath();
        CancelInvoke(nameof(ResetAttacking));
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
        Invoke(nameof(ResetHit), 2f);
    }

    void ResetHit() => isHit = false;

    public void ApplyAttackDamage()
    {
        if (!isAttacking) return;
        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
            playerHealth?.TakeDamage(attackDamage);
    }

    // ─── Utilitários ──────────────────────────────────────────────────────────

    Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, 1);
        return hit.position;
    }

    void OnDrawGizmosSelected()
    {
        // Cone de visão
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Vector3 left  = Quaternion.Euler(0, -visionAngle, 0) * transform.forward * visionRange;
        Vector3 right = Quaternion.Euler(0,  visionAngle, 0) * transform.forward * visionRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);

        // Proximidade
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, proximityRange);

        // Alcance de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Último lugar visto
        if (currentState == State.Investigate)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPosition, 0.3f);
        }
    }
}