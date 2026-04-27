using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Dead }
    public State currentState = State.Idle;

    [Header("Patrulha")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    private float patrolTimer;
    private Vector3 patrolTarget;

    [Header("Detecção")]
    public float detectionRange = 10f;
    public float attackRange    = 1.8f;

    [Header("Combate")]
    public float attackDamage   = 10f;
    public float attackCooldown = 1.5f;
    public float attackDelay    = 0.5f; // delay até o hit real (no meio da animação)

    [Header("Velocidade")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Referências")]
    public Transform player;
    public LayerMask playerLayer;

    [Header("SFX")]
    public AudioSource footstepSource;
    public AudioSource patrolGrowl;
    public AudioSource chaseGrowl;
    public AudioSource Swing;


    [Header("SFX Timers")]
    private float nextPatrolSoundTime = 0f;
    private float nextChaseGrowlTime = 0f;

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

        HandleMovementSFX();
    }

    void UpdateState(float dist)
    {
        if (dist <= attackRange)
            currentState = State.Attack;
        else if (dist <= detectionRange)
            currentState = State.Chase;
        else
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
                agent.isStopped = false;
                agent.speed = patrolSpeed;
                patrolTimer -= Time.deltaTime;

                if (patrolTimer <= 0f || agent.remainingDistance < 0.5f)
                {
                    patrolTarget = GetRandomPatrolPoint();
                    agent.SetDestination(patrolTarget);
                    patrolTimer = patrolWaitTime;
                }

                if (Time.time >= nextPatrolSoundTime)
                {
                    patrolGrowl.PlayOneShot(patrolGrowl.clip);
                    nextPatrolSoundTime = Time.time + Random.Range(4f, 10f);
                }                
                break;

            case State.Chase:
                if (Time.time >= nextChaseGrowlTime)
                {
                    chaseGrowl.PlayOneShot(chaseGrowl.clip);
                    nextChaseGrowlTime = Time.time + Random.Range(3f, 5f);
                }
                animator.ResetTrigger("Attack");
                agent.isStopped = false;
                agent.speed = chaseSpeed;
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

            if (attackTimer <= 0f && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    Debug.Log($"Trigger Attack setado | Estado Animator: {animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")}");
                    attackTimer = attackCooldown;
                    Swing.PlayDelayed(0.6f);
                    patrolGrowl.PlayDelayed(0.0f);
                    animator.Play("Attack", 0, 0f); 
                }
                break;
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
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

    Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDir, out hit, patrolRadius, 1);
        return hit.position;
    }


    void HandleMovementSFX()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            if (!footstepSource.isPlaying)
                footstepSource.Play();

            footstepSource.pitch = currentState == State.Chase ? 1.25f : 1.1f;
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
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