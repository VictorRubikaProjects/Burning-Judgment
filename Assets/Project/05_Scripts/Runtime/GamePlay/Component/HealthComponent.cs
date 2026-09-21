using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class HealthComponent : ActorComponent, IDamageable
{
    private readonly PlayerCharacter m_pc;

    private const float InvincibilityDuration = 0.5f;

    private bool m_isInvincible;
    private CancellationTokenSource m_invincibilityCts;

    public HealthComponent(Actor owner) : base(owner)
    {
        m_pc = owner as PlayerCharacter;
    }

    public bool TakeDamage()
    {
        if (!CanTakeDamage()) return false;
        
        m_pc.Aspect.HurtVisuals();
        StartInvincibility();
        return true;
    }

    public bool CanTakeDamage() => !m_isInvincible;

    public override void Dispose()
    {
        base.Dispose();
        m_invincibilityCts?.Cancel();
        m_invincibilityCts?.Dispose();
    }

    private void StartInvincibility()
    {
        m_invincibilityCts?.Cancel();
        m_invincibilityCts = new CancellationTokenSource();
        InvincibilityAsync(m_invincibilityCts.Token).Forget();
    }

    private async UniTaskVoid InvincibilityAsync(CancellationToken token)
    {
        m_isInvincible = true;
        await UniTask.Delay(TimeSpan.FromSeconds(InvincibilityDuration), cancellationToken: token);
        m_isInvincible = false;
    }
}