using UnityEngine;

public class EnemyShooterFleeState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    
    private EnemyShooter m_owner;
    
    public EnemyShooterFleeState(EnemyShooter owner)
    {
        m_owner = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        m_owner.Shoot.Enable(true);
        m_owner.ConsumeFleeRequest();
        IsFinished = false;
        m_owner.Agent.stoppingDistance = 0f;
        CalculateFleePosition();
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"[{m_owner.name}] Exit Flee");
        m_owner.Shoot.Enable(false);
    }

    public override void Update()
    {
        base.Update();
        m_owner.LookAtPlayer();
        CalculateFleePosition();
        EvaluateFleeSuccess();
    }

    private void CalculateFleePosition()
    {
        Vector3 directionAwayFromPlayer = (m_owner.TransformCache.position - m_owner.Player.TransformCache.position).normalized;
        Vector3 fleeTarget = m_owner.TransformCache.position + directionAwayFromPlayer * m_owner.Stats.DistanceMax;
        m_owner.Agent.SetDestination(fleeTarget);
    }

    private void EvaluateFleeSuccess()
    {
        float distance = Vector3.Distance(m_owner.TransformCache.position,m_owner.Player.TransformCache.position);
    
        if (distance > m_owner.Stats.DistanceMin)
        {
            Debug.Log($"[{m_owner.name}] Flee finished, distance={distance}");
            IsFinished = true;
            return;
        }
    }
}