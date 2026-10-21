using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    private readonly Rigidbody m_rb;

    private CancellationTokenSource m_ctsDash;
    
    private AudioService m_audioService;

    public bool IsFinished { get; private set; }

    public PlayerRollState(PlayerCharacter owner, Animator animator, Rigidbody rb, SO_PlayerStats playerStats)
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
        
        RollAsync(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
    }
    
    private async UniTaskVoid RollAsync(Vector3 direction, CancellationToken token)
    {
        m_audioService.PlaySfx(m_stats.DashEvent);

        bool dashSucceeded = await m_owner.Dash.RollAsync(
            direction,
            token,
            m_stats.DashDuration,
            m_stats.DashDistance,
            m_stats.DashCurve,
            m_stats.ObstacleLayer);

        IsFinished = true;
    }
}