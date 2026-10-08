using UnityEngine;

public class PlayerRecoverState : PlayerBaseState
{
    private readonly CountdownTimer m_recoverTimer;
    private readonly CountdownTimer m_cancelTimer;

    private bool m_isFinished;

    public bool IsFinished => m_isFinished;

    public PlayerRecoverState(
        PlayerCharacter owner,
        Animator animator,
        SO_PlayerStats playerStats
    ) : base(owner, animator, playerStats)
    {
        m_recoverTimer = new CountdownTimer(m_stats.RecoverDuration);
        m_cancelTimer = new CountdownTimer(m_stats.RecoverCancelThreshold);
    }

    public override void OnEnter()
    {
        m_isFinished = false;

        m_recoverTimer.Reset(m_stats.RecoverDuration);
        m_cancelTimer.Reset(m_stats.RecoverCancelThreshold);

        m_recoverTimer.OnTimerStop += RecoverOver;
        m_cancelTimer.OnTimerStop += CantRecover;

        m_recoverTimer.Start();
        m_cancelTimer.Start();

        m_owner.SetCanCancelRecover(true);
    }

    public override void OnExit()
    {
        m_recoverTimer.OnTimerStop -= RecoverOver;
        m_cancelTimer.OnTimerStop -= CantRecover;

        m_recoverTimer.Stop();
        m_cancelTimer.Stop();

        m_owner.SetCanCancelRecover(false);
    }

    private void RecoverOver() => m_isFinished = true;

    private void CantRecover() => m_owner.SetCanCancelRecover(false);
}