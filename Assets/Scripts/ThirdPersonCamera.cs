using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target; // arraste o CameraTarget aqui

    [Header("Configurações")]
    public float distance      = 5f;
    public float minDistance   = 1f;
    public float sensitivity   = 3f;
    public float verticalMin   = -20f;
    public float verticalMax   = 60f;

    [Header("Colisão")]
    public LayerMask collisionLayers;
    public float collisionOffset = 0.2f;

    private float yaw;
    private float pitch = 15f;

    void Awake()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void LateUpdate()
    {
        yaw   += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch  = Mathf.Clamp(pitch, verticalMin, verticalMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * new Vector3(0, 0, -distance);

        // Raycast para colisão
        float actualDistance = distance;
        RaycastHit hit;
        Vector3 dir = (desiredPosition - target.position).normalized;

        if (Physics.SphereCast(target.position, collisionOffset, dir, out hit, distance, collisionLayers))
        {
            actualDistance = Mathf.Clamp(hit.distance - collisionOffset, minDistance, distance);
        }

        transform.position = target.position + rotation * new Vector3(0, 0, -actualDistance);
        transform.LookAt(target.position);
    }
}