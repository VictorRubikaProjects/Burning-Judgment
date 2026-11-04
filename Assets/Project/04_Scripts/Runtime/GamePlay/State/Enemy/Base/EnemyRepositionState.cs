using UnityEngine;

public class EnemyRepositionState : EnemyBaseState
{
    private const int CandidateCount = 12;
    private const float RefreshInterval = 0.4f;
    private const float ReachedSqrDistance = 0.09f;
    private const float BlockedPathPenalty = 10000f;

    public bool IsFinished { get; private set; }

    private Vector3 m_targetPosition;
    private bool m_hasTarget;
    private float m_refreshTimer;

    public EnemyRepositionState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
        Owner.ConsumeRepositionRequest();
        RefreshTarget();
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();

        m_refreshTimer -= Time.deltaTime;
        if (m_refreshTimer <= 0f) RefreshTarget();

        EvaluateFinish();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!m_hasTarget || HasReachedTarget()) return;

        Owner.Steering.MoveTowards(m_targetPosition, Owner.StatsBase.Speed);
    }

    public override void OnExit()
    {
        base.OnExit();
        Owner.Steering.Stop();
    }

    private void RefreshTarget()
    {
        m_refreshTimer = RefreshInterval;
        m_hasTarget = TryFindPosition(out m_targetPosition);
    }

    private bool HasReachedTarget()
    {
        Vector3 toTarget = m_targetPosition - Owner.TransformCache.position;
        toTarget.y = 0f;
        return toTarget.sqrMagnitude < ReachedSqrDistance;
    }

    private void EvaluateFinish()
    {
        EnemyBase.PositionNeed need = Owner.EvaluatePositionNeed();

        if (need == EnemyBase.PositionNeed.Reposition) return;
        if (need == EnemyBase.PositionNeed.None && m_hasTarget && !HasReachedTarget()) return;

        IsFinished = true;
    }

    private bool TryFindPosition(out Vector3 position)
    {
        Vector3 ownerPosition = Owner.TransformCache.position;
        Vector3 playerPosition = Owner.Player.TransformCache.position;
        float radius = Owner.ComfortDistance;
        float bestScore = float.MaxValue;
        bool found = false;

        position = ownerPosition;

        for (int i = 0; i < CandidateCount; i++)
        {
            float angle = i * (360f / CandidateCount) * Mathf.Deg2Rad;
            Vector3 candidate = playerPosition + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            candidate.y = ownerPosition.y;

            if (!Owner.GroundCheck.IsGrounded(candidate)) continue;
            if (!Owner.HasSightFrom(candidate)) continue;

            float score = (candidate - ownerPosition).sqrMagnitude;
            if (IsPathBlocked(ownerPosition, candidate)) score += BlockedPathPenalty;
            if (score >= bestScore) continue;

            bestScore = score;
            position = candidate;
            found = true;
        }

        return found;
    }

    private bool IsPathBlocked(Vector3 from, Vector3 to)
    {
        Vector3 offset = Vector3.up * Owner.StatsBase.SightHeight;
        return Physics.Linecast(from + offset, to + offset, Owner.StatsBase.ObstacleLayer, QueryTriggerInteraction.Ignore);
    }
}