using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float rotationSpeed = 10f;

    [Header("Velocidade")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Referências")]
    public Camera cameraTransform;

    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        
        if (cameraTransform == null)
            cameraTransform = Camera.main;
    }

    void Update()
    {
        // HandleInput();
        HandleRunning();
        HandleInteraction();
        HandleAttack();
    }


    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        
        animator.SetFloat("Horizontal", h);
        animator.SetFloat("Vertical", v);

        if(h != 0 || v != 0)
        {
            animator.SetBool("IsWalking", true);
        } else
        {
            animator.SetBool("IsWalking", false);
        }

        Vector3 camForward = cameraTransform.transform.forward;
        Vector3 camRight   = cameraTransform.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * v + camRight * h).normalized;
        Move();
    }

    void HandleInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Direção relativa à câmera
        Vector3 camForward = cameraTransform.transform.forward;
        Vector3 camRight   = cameraTransform.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * v + camRight * h).normalized;
    }

    void Move()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[Player] Interação ativada!");
            // Aqui entrará a lógica real de interação futuramente
        }
    }

    void HandleRunning()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetBool("IsRunning", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.SetBool("IsRunning", false);
        }
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[Player] Ataque executado!");
            RaycastHit hit;

            Debug.Log($"Origem: {transform.position + Vector3.up} | Direção: {transform.forward}");

            if (Physics.SphereCast(transform.position + Vector3.up, 0.5f, transform.forward, out hit, 2f))
            {
                Debug.Log($"Raycast acertou: {hit.collider.gameObject.name}");

                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                    enemy.TakeDamage(25f);
                else
                    Debug.Log("Objeto acertado não tem EnemyHealth");
            }
            else
            {
                Debug.Log("Raycast não acertou nada");
            }
        }
    }
}