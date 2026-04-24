using UnityEngine;

public class EnemyAttackEvent : MonoBehaviour
{
    private EnemyAI ai;

    void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    // Este método será chamado diretamente pela Animation Event
    public void OnAttackHit()
    {
        ai.ApplyAttackDamage();
    }
}