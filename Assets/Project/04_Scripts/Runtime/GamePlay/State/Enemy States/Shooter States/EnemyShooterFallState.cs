using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyShooterFallState : EnemyBaseState
{
    private EnemyShooter m_owner;

    public EnemyShooterFallState(EnemyShooter owner)
    {
        m_owner = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();

       FallAsync().Forget();
    }

    private async UniTaskVoid FallAsync()
    {
        m_owner.Shoot.Enable(false);
        m_owner.KnockBack.Cancel();

        m_owner.Agent.enabled = false;

        m_owner.Rigidbody.isKinematic = false;
        m_owner.Rigidbody.useGravity = true;

        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        
        m_owner.Rigidbody.linearVelocity = Vector3.zero;
        m_owner.Rigidbody.angularVelocity = Vector3.zero;
    }

}