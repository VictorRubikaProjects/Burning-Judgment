using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashState : BaseState
{
    private readonly Rigidbody m_rb;

    private CancellationTokenSource m_ctsDash;
    
    private Collider[] m_overlapBuffer = new Collider[100];

    public bool IsFinished { get; private set; }

    public DashState(PlayerCharacter owner, Animator animator, Rigidbody rb, ConfigStatsPlayer configStats)
        : base(owner, animator, configStats)
    {
        m_rb = rb;
    }

    public override void OnEnter()
    {
        IsFinished = false;
        
        m_animator.CrossFade(DashHash, m_crossFadeDuration);

        m_owner.ConsumeDashRequest();

        m_ctsDash = new CancellationTokenSource();
        
        DashAsync(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        ExecuteStrike();
    }
    
    private async UniTaskVoid DashAsync(Vector3 direction, CancellationToken token)
    {
        await m_owner.Controller.DashAsync(direction,
            token,
            m_configStats.DashDuration,
            m_configStats.DashDistance,
            m_configStats.DashCurve);
        
        IsFinished = true;
    }
    
    private void ExecuteStrike()
    {
        Vector3 start = m_rb.position + m_owner.MoveDir * m_configStats.OffsetDash;
        
        int count = Physics.OverlapSphereNonAlloc(
            start,
            m_configStats.DashRadius,
            m_overlapBuffer,
            m_configStats.EnemyLayer);
        
        for (int i = 0; i < count; i++)
        {
            Collider hit = m_overlapBuffer[i];
            
            if (hit == null) continue;

            if (!hit.TryGetComponent(out IDamageable damageable) || !damageable.CanTakeDamage()) continue;

            m_ctsDash.Cancel();
            
            IsFinished = true;
            
            return;
        }
    }
    
}