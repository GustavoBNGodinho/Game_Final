using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Referências")]
    public Camera cameraTransform;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (cameraTransform == null)
            cameraTransform = Camera.main;
    }

    void Update()
    {
        // HandleInput();
        HandleInteraction();
        HandleAttack();
    }


    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

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
            // Rotaciona o player para a direção do movimento
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        Vector3 velocity = moveDirection * moveSpeed;
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

void HandleAttack()
{
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("[Player] Ataque executado!");

        // Raycast para frente do player para detectar inimigo
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, 2f))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(25f);
            }
        }
    }
}
}