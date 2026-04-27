using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float rotationSpeed = 10f;

    [Header("SFX")]
    public AudioSource footstepSource;
    public AudioSource clothesSource;
    public AudioSource pistolFire;
    public AudioSource pistolDry;
    public AudioSource pistolDraw;

    [Header("Velocidade")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Combate")]
    public bool isArmed = false;

    [Header("Configuração de Tiro")]
    public TextMeshProUGUI tmpBullet;
    private float quantBullet = 12;

    [Header("Efeito Visual")]
    public LineRenderer tiroLinha; // Arraste o objeto "LinhaDoTiro" para cá
    public float tempoExibicaoLinha = 0.05f;

    [Header("Referências")]
    public Camera cameraTransform;

    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    public GameObject pistola;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        pistola.SetActive(isArmed);
        if (cameraTransform == null)
            cameraTransform = Camera.main;
    }

    void Update()
    {
        // HandleInput();
        HandleRunning();
        HandleInteraction();
        HandleAttack();
        HandleGun();
        SetUI();
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
        HandleMoveSFX();
    }

    void HandleGun()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isArmed)
            {
                animator.SetBool("WithGun", false);
                Debug.Log("[Player] não esta Armado!");
                isArmed = false;
                pistola.SetActive(isArmed);
            }
            else
            {
                GunDrawSFX();
                animator.SetBool("WithGun", true);
                Debug.Log("[Player] esta Armado!");
                isArmed = true;
                pistola.SetActive(isArmed);
            }

        }
    }

    void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isArmed)
            {
                if (quantBullet != 0)
                {
                    GunFireSFX();
                    ReduceBullet();
                    AttackArmed();
                    
                }
                else
                {
                    GunDrySFX();
                }


            }
            else
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

    void ReduceBullet()
    {
        quantBullet = Mathf.Clamp(quantBullet -= 1, 0, 99);
    }

    public void AddBullet(float valeu)
    {
        quantBullet = Mathf.Clamp(quantBullet += valeu, 0, 99);
    }

    void SetUI()
    {
        tmpBullet.text = quantBullet.ToString();
    }

    void AttackArmed()
    {
        Debug.Log("[Player] Ataque armado!");
        animator.SetTrigger("AttackArmed");
        ShootBullet();
        // lógica de arma virá aqui futuramente
    }

    public void ShootBullet()
    {
        Debug.Log("[Player] Bala aTIRADA:");
        RaycastHit hit;
        Vector3 pontoFinal;
        // Dispara um raio fino (ou esfera) para frente a partir do firePoint
        // Raio de 0.1f para ser preciso como uma bala, alcance de 50 metros
        if (Physics.SphereCast(transform.position + Vector3.up, 0.5f, transform.forward, out hit, 50f))
        {
            pontoFinal = hit.point;

            Debug.Log($"[Player] Bala atingiu: {hit.collider.name}");

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(25f);
                Debug.Log("[Player] Dano de tiro contabilizado no inimigo!");
            }
        }
        else
        {
            pontoFinal = transform.position + Vector3.up + (transform.forward * 50f);
        }

        StartCoroutine(MostrarFeixe(pontoFinal));
    }

    void HandleMoveSFX()
    {
        bool isMoving = moveDirection.magnitude > 0.1f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isMoving)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }

            footstepSource.pitch = isRunning ? 1.3f : 1f;


            if (!clothesSource.isPlaying)
            {
                clothesSource.Play();
            }
            clothesSource.pitch = isRunning ? 1.6f : 1f;
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }

            if (clothesSource.isPlaying)
            {
                clothesSource.Stop();
            }
        }
    }

    void GunFireSFX()
    {
        pistolFire.PlayOneShot(pistolFire.clip);
    }


    void GunDrySFX()
    {
        pistolDry.PlayOneShot(pistolDry.clip);
    }

    void GunDrawSFX()
    {
        pistolDraw.Play();
    }


    System.Collections.IEnumerator MostrarFeixe(Vector3 destino)
    {
        tiroLinha.SetPosition(0, transform.position + Vector3.up); // Começo no cano
        tiroLinha.SetPosition(1, destino);            // Fim no alvo
        tiroLinha.enabled = true;

        yield return new WaitForSeconds(tempoExibicaoLinha);

        tiroLinha.enabled = false;
    }
}