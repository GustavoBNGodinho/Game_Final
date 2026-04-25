using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float rotationSpeed = 10f;

    [Header("Velocidade")]
    public float walkSpeed = 3f;  // substituímos o moveSpeed fixo pelo sistema walk/run
    public float runSpeed = 6f;

    [Header("Combate")]
    public bool isArmed = false;

    [Header("Referências")]
    public Camera cameraTransform;

    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isDead = false;
    public bool isAttacking = false;
    private bool isHit = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>(); // busca o Animator no filho (modelo)
        rb = GetComponent<Rigidbody>();

        if (cameraTransform == null)
            cameraTransform = Camera.main;

        // Debug.Log($"Animator encontrado: {animator?.gameObject.name}");
    }

    void Update()
    {
        if (isDead) return;
        if (isHit) return;
        HandleRunning();
        HandleInteraction();
        HandleAttack();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", h);
        animator.SetFloat("Vertical", v);
        animator.SetBool("IsWalking", !isHit && (h != 0 || v != 0));

        Vector3 camForward = cameraTransform.transform.forward;
        Vector3 camRight   = cameraTransform.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * v + camRight * h).normalized;

        if (!isAttacking && !isHit)
            Move();
    }

    void Move()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        // velocidade dinâmica walk/run que implementamos
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void HandleRunning()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            animator.SetBool("IsRunning", true);
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            animator.SetBool("IsRunning", false);
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Debug.Log("[Player] Interação ativada!");
        }
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isArmed)
                AttackArmed();
            else
                AttackUnarmed();
        }
    }

    void AttackUnarmed()
    {   
        // Debug.Log($"[Player] AttackUnarmed | isAttacking: {isAttacking} | isHit: {isHit}");
        if (isAttacking || isHit) return;
            isAttacking = true;
            moveDirection = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            animator.SetTrigger("AttackUnarmed");
            Invoke(nameof(ResetAttack), 1.5f); // fallback caso o evento falhe
    }

    // Chamado pelo Animation Event no frame do impacto
    public void OnUnarmedHit()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up, 0.5f, transform.forward, out hit, 2f))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(25f);
                // Debug.Log("[Player] Acertou o inimigo!");
            }
        }
    }

    void AttackArmed()
    {
        // Debug.Log("[Player] Ataque armado!");
        animator.SetTrigger("AttackArmed");
        // lógica de arma virá aqui futuramente
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    public void OnHit()
    {
        isHit = true;
        isAttacking = false;
        CancelInvoke(nameof(ResetAttack));
        moveDirection = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        animator.ResetTrigger("Hit");
        animator.SetTrigger("Hit");
        Invoke(nameof(ResetHit), 0.5f);
        // Debug.Log($"[Player] OnHit chamado | isHit: {isHit} | estado animator: {animator.GetCurrentAnimatorStateInfo(0).IsName("Hit")}");
    }

    void ResetHit()
    {
        animator.ResetTrigger("Hit");
        isHit = false;
    }

    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        isAttacking = false;
        isHit = false;
        CancelInvoke();
        moveDirection = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
        animator.SetTrigger("Death");
        enabled = false;
    }
}