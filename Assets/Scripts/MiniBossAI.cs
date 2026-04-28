using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class MiniBossAI : MonoBehaviour
{
    public enum State { Idle, Intro, Patrol, Chase, Investigate, Attack, Dead }
    public State currentState = State.Idle;

    [Header("Patrulha")]
    public float patrolRadius   = 10f;
    public float patrolWaitTime = 2f;
    private float   patrolTimer;
    private Vector3 patrolTarget;

    [Header("Detecção — Cone de Visão")]
    public float visionRange = 20f;
    public float visionAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Detecção — Proximidade")]
    public float proximityRange = 4f;

    [Header("Combate")]
    public float attackRange    = 3.5f;
    public float attackDamage   = 25f;
    public float attackCooldown = 5f;

    [Header("Duração dos Clipes — Timeout de Segurança")]
    public float attack1Duration = 2.0f;
    public float attack2Duration = 2.5f;
    public float attack3Duration = 3.0f;
    public float hitAnimDuration = 2.0f;

    [Header("Velocidade")]
    public float patrolSpeed      = 2f;
    public float chaseSpeed       = 4f;
    public float investigateSpeed = 3f;

    [Header("Investigação")]
    public float investigateWaitTime = 4f;
    private Vector3 lastKnownPosition;
    private float   investigateTimer;

    [Header("Referências")]
    public Transform player;

    [Header("Perseguição")]
    public float pathUpdateInterval = 0.15f;
    private float pathUpdateTimer   = 0f;

    [Header("Reação a Dano")]
    public float reactionThreshold  = 30f; // dano necessário para reagir
    public float reactionWindow     = 5f;  // janela de tempo em segundos
    public float damageTaken       = 0f;  // acumulado na janela atual
    public float damageWindowTimer = 0f;  // tempo restante da janela

    [Header("Ativação")]
    public float activationRange   = 10f;
    public float introAnimDuration = 2.4f; // duração do GetUp
    public float screamAnimDuration = 2f; // duração do ZombieScream
    private bool isActive          = false;

    [Header("SFX")]
    public AudioSource bossStep;
    public AudioSource bossFeeding;
    public AudioSource slam;
    public AudioSource screech;
    public AudioSource grunt;
    public AudioSource swing;

    private NavMeshAgent agent;
    private Animator     animator;
    private PlayerHealth playerHealth;

    private float attackTimer    = 0f;
    private float attackingTimeout = 0f;
    private float hittingTimeout   = 0f;

    public bool isAttacking = false;
    public bool isHit       = false;

    void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.isStopped = true; // garante parado antes de qualquer coisa

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

    }

    void Update()
    {
        if (currentState == State.Dead) return;

        // Ativação — checa uma única vez
        if (!isActive)
        {
            if (Vector3.Distance(transform.position, player.position) <= activationRange)
            {
                isActive = true;
                currentState = State.Intro; // bloqueia a state machine
                animator.SetBool("IsActive", true);
                StartCoroutine(ActivateAfterIntro());
            }
            HandleIdleSFX();
            return;
        }

        attackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        TickTimeouts();
        UpdateState(dist);
        ExecuteState(dist);
        UpdateAnimator();

        HandleMovementSFX();
    }

    IEnumerator ActivateAfterIntro()
    {
        agent.isStopped = true;
        agent.velocity  = Vector3.zero;
        screech.PlayDelayed(introAnimDuration - 0.4f);
        yield return new WaitForSeconds(introAnimDuration); // duração do GetUp
        yield return new WaitForSeconds(screamAnimDuration); // duração do ZombieScream
        agent.isStopped = false;
        currentState = State.Patrol;
    }

    void LateUpdate()
    {
        if (isHit || isAttacking)
        {
            agent.isStopped = true;
            agent.velocity  = Vector3.zero;
        }
    }

    // ─── Detecção ─────────────────────────────────────────────────────────────

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float   angle       = Vector3.Angle(transform.forward, dirToPlayer);
        float   dist        = Vector3.Distance(transform.position, player.position);

        if (dist > visionRange) return false;
        if (angle > visionAngle) return false;

        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 target = player.position    + Vector3.up * 1f;

        if (Physics.Raycast(origin, (target - origin).normalized, dist, obstacleMask))
            return false;

        return true;
    }

    bool CanSensePlayer()
    {
        return Vector3.Distance(transform.position, player.position) <= proximityRange;
    }

    bool PlayerDetected() => CanSeePlayer() || CanSensePlayer();

    // ─── State Machine ────────────────────────────────────────────────────────

    void UpdateState(float dist)
    {
        if (currentState == State.Intro) return; // intro tem prioridade absoluta
        if (isAttacking || isHit) return;

        if (dist <= attackRange && PlayerDetected())
        {
            currentState = State.Attack;
            return;
        }

        if (PlayerDetected())
        {
            lastKnownPosition = player.position;
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
            case State.Intro:
                agent.isStopped = true;
                agent.velocity  = Vector3.zero;
                break;
                
            case State.Idle:
                agent.isStopped = true;
                break;

            case State.Patrol:
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

                animator.ResetTrigger("Attack1");
                animator.ResetTrigger("Attack2");
                animator.ResetTrigger("Attack3");
                agent.isStopped        = false;
                agent.speed            = chaseSpeed;
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
                agent.isStopped        = false;
                agent.speed            = investigateSpeed;
                agent.stoppingDistance = 0.5f;
                agent.SetDestination(lastKnownPosition);

                if (agent.remainingDistance < 0.6f)
                {
                    agent.isStopped   = true;
                    investigateTimer -= Time.deltaTime;
                    if (investigateTimer <= 0f)
                        currentState = State.Patrol;
                }
                break;

            case State.Attack:
                agent.isStopped = true;
                agent.velocity  = Vector3.zero;

                if (isHit) break;

                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

                if (attackTimer <= 0f && !isAttacking && attackingTimeout <= 0f)
                {
                    attackTimer = attackCooldown;

                    int chosen = Random.Range(0, 3);
                    switch (chosen)
                    {
                        case 0:
                            attackingTimeout = attack1Duration;
                            swing.PlayDelayed(0.3f);
                            animator.ResetTrigger("Attack2");
                            animator.ResetTrigger("Attack3");
                            animator.SetTrigger("Attack1");
                            break;
                        case 1:
                            attackingTimeout = attack2Duration;
                            slam.PlayDelayed(0.8f);
                            animator.ResetTrigger("Attack1");
                            animator.ResetTrigger("Attack3");
                            animator.SetTrigger("Attack2");
                            break;
                        case 2:
                            attackingTimeout = attack3Duration;
                            grunt.PlayDelayed(0.1f);
                            animator.ResetTrigger("Attack1");
                            animator.ResetTrigger("Attack2");
                            animator.SetTrigger("Attack3");
                            break;
                    }

                    isAttacking = true;
                }
                break;
        }
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    // ─── Timeouts ─────────────────────────────────────────────────────────────

    void TickTimeouts()
    {
        // Janela de dano — decai com o tempo
        if (damageWindowTimer > 0f)
        {
            damageWindowTimer -= Time.deltaTime;
            if (damageWindowTimer <= 0f)
                damageTaken = 0f; // janela expirou, reseta acumulador
        }

        if (isAttacking)
        {
            attackingTimeout -= Time.deltaTime;
            if (attackingTimeout <= 0f)
                isAttacking = false;
        }

        if (isHit)
        {
            hittingTimeout -= Time.deltaTime;
            if (hittingTimeout <= 0f)
                isHit = false;
        }
    }

    // ─── Animation Events ─────────────────────────────────────────────────────

    public void SetAttacking(int value)
    {
        isAttacking = value == 1;
    }

    public void SetHit(int value)
    {
        isHit = value == 1;
        if (isHit)
            hittingTimeout = hitAnimDuration;
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────

    public void ApplyAttackDamage()
    {
        if (!isAttacking) return;
        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
            playerHealth?.TakeDamage(attackDamage);
    }

    public void OnHit(float damage)
    {
        damageTaken       += damage;
        damageWindowTimer  = reactionWindow; // reseta a janela a cada hit

        if (damageTaken >= reactionThreshold)
        {
            damageTaken = 0f; // reseta acumulador
            TriggerReaction();
        }
    }

    void TriggerReaction()
    {
        Debug.Log("[MiniBoss] Threshold atingido — executando Reaction");
        isHit            = true;
        isAttacking      = false;
        attackingTimeout = 0f;
        hittingTimeout   = hitAnimDuration;
        currentState     = State.Idle;
        agent.velocity   = Vector3.zero;
        agent.isStopped  = true;
        agent.ResetPath();
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack3");
        animator.ResetTrigger("Reaction");
        animator.SetTrigger("Reaction");
    }
    // ─── SFX ──────────────────────────────────────────────────────────
    void HandleMovementSFX()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            if (!bossStep.isPlaying)
                bossStep.Play();
            bossStep.pitch = currentState == State.Chase ? 1.67f : 1.1f;
        }
        else
        {
            if (bossStep.isPlaying)
                bossStep.Stop();
        }
    }

    void HandleIdleSFX()
    {
        bool isIdle = (currentState == State.Idle);
        if (!isIdle)
        {
            if (bossFeeding.isPlaying)
            bossFeeding.Stop();
            return;
        }

        if (!bossFeeding.isPlaying)
        {
            bossFeeding.Play();
        }

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Vector3 left  = Quaternion.Euler(0, -visionAngle, 0) * transform.forward * visionRange;
        Vector3 right = Quaternion.Euler(0,  visionAngle, 0) * transform.forward * visionRange;
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, proximityRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (currentState == State.Investigate)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPosition, 0.3f);
        }
    }


}