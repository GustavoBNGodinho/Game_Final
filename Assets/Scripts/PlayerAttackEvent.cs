using UnityEngine;

public class PlayerAttackEvent : MonoBehaviour
{
    private PlayerController controller;

    void Awake()
    {
        controller = GetComponentInParent<PlayerController>();
    }

    public void OnUnarmedHit()
    {
        controller.OnUnarmedHit();
    }

    public void OnAttackEnd()
    {
        controller.isAttacking = false;
    }
}