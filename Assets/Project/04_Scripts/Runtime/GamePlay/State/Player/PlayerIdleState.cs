using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    private readonly float AttackAnimationDuration;
    private CancellationTokenSource m_cts;

    public PlayerIdleState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.IdleAnimation)
    {
        AttackAnimationDuration = 0.75f;
    }
    
    public override void OnEnter()
    {
        m_cts?.Cancel();
        m_cts?.Dispose();
        m_cts = new CancellationTokenSource();
        
        PlayIdleAsync(m_cts.Token).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        
        m_cts?.Cancel();
        m_cts?.Dispose();
        m_cts = null;
    }

    private async UniTask PlayIdleAsync(CancellationToken ct)
    {
        if (m_owner.IsPreviousStateDash() && m_owner.HasHitInDashState())
        {
            await UniTask.Delay(TimeSpan.FromSeconds(AttackAnimationDuration), cancellationToken: ct);
        }
        
        PlayAnimation(IdleHash);
    }
}