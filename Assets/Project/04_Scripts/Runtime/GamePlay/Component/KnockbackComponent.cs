using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class KnockbackComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private readonly Collider m_collider;
    private CancellationTokenSource m_cts;

    public bool IsActive { get; private set; }

    public KnockbackComponent(Actor owner, Rigidbody rb) : base(owner)
    {
        m_rb = rb;
        m_collider = rb.GetComponentInChildren<Collider>();
    }

    public override void Dispose()
    {
        base.Dispose();
        Cancel();
    }

    public void Cancel()
    {
        if (m_cts == null) return;

        m_cts.Cancel();
        m_cts.Dispose();
        m_cts = null;
        IsActive = false;
    }

    public async UniTask<bool> HitAsync(
        Vector3 direction,
        float duration,
        float distance,
        AnimationCurve curve,
        LayerMask obstacleLayer)
    {
        Cancel();

        CancellationTokenSource cts = new CancellationTokenSource();
        m_cts = cts;
        IsActive = true;

        bool completed;

        try
        {
            completed = await MovementComponentExtension.MoveAsync(
                m_rb,
                m_collider,
                direction,
                cts.Token,
                duration,
                distance,
                curve,
                obstacleLayer,
                MovementComponentExtension.CollisionMode.Slide);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        if (m_cts == cts) IsActive = false;

        return completed;
    }
}