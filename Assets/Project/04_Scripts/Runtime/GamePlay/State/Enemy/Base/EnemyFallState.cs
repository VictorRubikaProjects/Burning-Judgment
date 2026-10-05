using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyFallState : EnemyBaseState
{
    public EnemyFallState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        FallAsync().Forget();
    }

    private async UniTaskVoid FallAsync()
    {
        Owner.KnockBack.Cancel();
        Owner.Agent.enabled = false;
        Owner.Rigidbody.isKinematic = false;
        Owner.Rigidbody.useGravity = true;

        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

        Owner.Rigidbody.linearVelocity = Vector3.zero;
        Owner.Rigidbody.angularVelocity = Vector3.zero;
    }
}