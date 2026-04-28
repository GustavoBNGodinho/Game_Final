using UnityEngine;
using UnityEngine.AI;

public class GoatAI : MonoBehaviour
{
    public enum State { Idle, Follow, ChaseTarget, Attack, Dead }
    public State currentState = State.Idle;

    [Header("Referencias")]
    public Transform player;
    public Transform currentTarget;

    [Header("Combate")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.2f;
    public float attackRange = 2.0f;

    [Header("Nomes das Animacoes")]
    public string idleAnim = "idle";
    public string walkAnim = "walk_forward";
    public string runAnim = "run_forward";
    public string attackAnim = "butt";
    public string deathAnim = "death";

    [Header("SFX")]
    public AudioSource hoovesSource;

    private NavMeshAgent agent;
    private Animator animator;
    private float attackTimer;
    private string lastAnimation;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (currentState == State.Dead) return;

        attackTimer -= Time.deltaTime;

        DecideState();
        ExecuteState();
        UpdateAnimations();
        HandleMoveSFX();
    }

    void DecideState()
    {
        if (currentTarget != null)
        {
            // Pega o script de vida do inimigo
            EnemyHealth health = currentTarget.GetComponent<EnemyHealth>();

            // Se o inimigo morreu (Usando seu m�todo p�blico IsDead)
            if (health == null || health.IsDead())
            {
                currentTarget = null;
                return;
            }

            float distToEnemy = Vector3.Distance(transform.position, currentTarget.position);
            currentState = (distToEnemy <= attackRange) ? State.Attack : State.ChaseTarget;
            return;
        }

        // Se n�o tem alvo, segue o player
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        currentState = (distToPlayer > 3.5f) ? State.Follow : State.Idle;
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case State.Idle:
                agent.isStopped = true;
                break;

            case State.Follow:
                agent.isStopped = false;
                agent.speed = 3.5f;
                agent.stoppingDistance = 3.0f;
                agent.SetDestination(player.position);
                break;

            case State.ChaseTarget:
                agent.isStopped = false;
                agent.speed = 6.5f;
                agent.stoppingDistance = attackRange;
                agent.SetDestination(currentTarget.position);
                break;

            case State.Attack:
                agent.isStopped = true;
                LookAtTarget(currentTarget);

                if (attackTimer <= 0f)
                {
                    attackTimer = attackCooldown;
                    PlayAnim(attackAnim);

                    // Aplica o dano no inimigo
                    EnemyHealth health = currentTarget.GetComponent<EnemyHealth>();
                    if (health != null) health.TakeDamage(attackDamage);
                }
                break;
        }
    }

    void UpdateAnimations()
    {
        // 1. Bloqueia troca de anima��o durante o "impacto" do ataque
        if (currentState == State.Attack && attackTimer > (attackCooldown - 0.4f)) return;

        // 2. Se a cabra estiver perto do player (Idle), for�a a anima��o de parado
        // Isso evita que ela fique "sambando" ou andando no lugar
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (currentState == State.Idle || (currentState == State.Follow && distToPlayer <= agent.stoppingDistance + 0.1f))
        {
            PlayAnim(idleAnim);
            return;
        }

        // 3. Se n�o estiver em Idle, decide entre andar ou correr baseado na velocidade
        // Adicionamos uma margem (0.5f) para garantir que ela realmente se moveu
        if (agent.velocity.magnitude < 0.5f)
        {
            PlayAnim(idleAnim);
        }
        else
        {
            string clip = (agent.speed > 4f) ? runAnim : walkAnim;
            PlayAnim(clip);
        }
    }

    void HandleMoveSFX()
    {
        bool isMoving = agent.velocity.magnitude > 0.5f;
        bool isRunning = agent.speed > 3.5f;

        if (isMoving)
        {
            if (!hoovesSource.isPlaying)
            {
                hoovesSource.Play();
            }

            hoovesSource.pitch = isRunning ? 1.3f : 1f;
        }
        else
        {
            if (hoovesSource.isPlaying)
            {
                hoovesSource.Stop();
            }
        }
        Debug.Log(isMoving);
    }

    void PlayAnim(string animName)
    {
        if (lastAnimation == animName) return;
        animator.CrossFade(animName, 0.2f);
        lastAnimation = animName;
    }

    void LookAtTarget(Transform t)
    {
        if (t == null) return;
        Vector3 dir = (t.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);
    }

    public void SetCombatTarget(Transform enemy)
    {
        if (currentState == State.Dead) return;
        currentTarget = enemy;
    }
}
