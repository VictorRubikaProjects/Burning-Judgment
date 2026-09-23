using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private readonly Rigidbody m_rb;

    private CancellationTokenSource m_ctsDash;
    
    private Collider[] m_overlapBuffer = new Collider[100];
    
    private AudioService m_audioService;

    public bool IsFinished { get; private set; }

    public PlayerDashState(PlayerCharacter owner, Animator animator, Rigidbody rb, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats)
    {
        m_rb = rb;
        m_audioService = ServiceLocator.Get<AudioService>();
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
        m_audioService.PlaySfx(m_stats.DashEvent);

        bool dashSucceeded = await m_owner.Controller.DashAsync(
            direction,
            token,
            m_stats.DashDuration,
            m_stats.DashDistance,
            m_stats.DashCurve);

        IsFinished = true;
    }
    
    private void ExecuteStrike()
    {
        Vector3 start = m_rb.position + m_owner.MoveDir * m_stats.OffsetDash;
        
        int count = Physics.OverlapSphereNonAlloc(
            start,
            m_stats.DashRadius,
            m_overlapBuffer,
            m_stats.EnemyLayer);
        
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