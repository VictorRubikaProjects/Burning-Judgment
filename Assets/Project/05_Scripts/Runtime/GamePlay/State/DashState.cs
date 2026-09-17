using System.Threading;
using Cysharp.Threading.Tasks;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class DashState : BaseState
{
    private readonly Rigidbody m_rb;
    private readonly float m_dashDistance;
    private readonly float m_dashDuration;
    private readonly AnimationCurve m_dashCurve;

    private CancellationTokenSource m_ctsDash;
    
    private Collider[] m_overlapBuffer = new Collider[100];

    public bool IsFinished { get; private set; }

    public DashState(PlayerCharacter owner, Animator animator, Rigidbody rb, ConfigStatsPlayer configStats)
        : base(owner, animator, configStats)
    {
        m_rb = rb;
        m_dashDistance = configStats.dashDistance;
        m_dashDuration = configStats.dashDuration;
        m_dashCurve = configStats.dashCurve;
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
        await m_owner.ControllerComponent.DashAsync(direction,token,m_dashDuration,m_dashDistance,m_dashCurve);
        IsFinished = true;
    }
    
    //TODO need rework OverlapSphere Gizmo + should be in front of player
    private void ExecuteStrike()
    {
        
        Vector3 start = m_rb.position;
        
        float radius = 2f;
        
        int count = Physics.OverlapSphereNonAlloc(start, radius, m_overlapBuffer, m_configStats.enemyLayer);
        
        for (int i = 0; i < count; i++)
        {
            Collider hit = m_overlapBuffer[i];
            
            if (hit == null) continue;

            if (!hit.TryGetComponent(out IDamageable damageable) || !damageable.CanTakeDamage()) continue;
            
            damageable.TakeDamage();

            m_ctsDash.Cancel();

            ServiceLocator.Get<CameraService>().Shake();
            
            IsFinished = true;
            
            return;
        }
    }
    
}