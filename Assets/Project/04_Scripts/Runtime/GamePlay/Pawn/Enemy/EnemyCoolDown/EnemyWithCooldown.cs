using UnityEngine;

public abstract class EnemyWithCooldown<TStats> : Enemy<TStats> where TStats : SO_ConfigEnemy
{
    protected float m_nextAttackTime;

    public void EvaluateAttackRequest() => m_attackRequested = CanAttack();

    public void ClearAttackRequest() => m_attackRequested = false;

    public void ConsumeAttackRequest(float cooldown)
    {
        m_attackRequested = false;
        m_nextAttackTime = Time.time + cooldown;
    }

    protected virtual bool CanAttack()
    {
        if (Time.time < m_nextAttackTime) return false;
        if (!GroundCheck.IsGrounded()) return false;
        if (!IsPlayerInSight()) return false; 

        float sqrDistance = (Player.TransformCache.position - TransformCache.position).sqrMagnitude;
        float min = Stats.DistanceMin;
        float max = Stats.DistanceMax;

        return sqrDistance >= min * min && sqrDistance <= max * max;
    }
}