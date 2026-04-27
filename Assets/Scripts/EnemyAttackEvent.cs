using UnityEngine;

public class EnemyAttackEvent : MonoBehaviour
{
    private EnemyAI ai;

    void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    public void OnAttackStart()
    {
        ai.isAttacking = true;
        // Debug.Log("OnAttackStart foi chamada");
    }

    // Este método será chamado diretamente pela Animation Event
    public void OnAttackHit()
    {
        ai.ApplyAttackDamage();
    }

    public void OnAttackEnd()
    {
        // Debug.Log("OnAttackEnd foi chamada");
        ai.isAttacking = false;
    }
}