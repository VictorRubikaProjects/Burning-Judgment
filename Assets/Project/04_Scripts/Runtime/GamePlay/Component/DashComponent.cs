using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private readonly GroundCheckComponent m_groundCheck;
    private readonly Collider m_collider;

    public bool IsDashing { get; private set; }

    public DashComponent(Actor owner, Rigidbody rb, GroundCheckComponent groundCheck) : base(owner)
    {
        m_rb = rb;
        m_groundCheck = groundCheck;
        m_collider = rb.GetComponentInChildren<Collider>();
    }

    public UniTask<bool> DashAsync(
        Vector3 direction,
        CancellationToken token,
        float duration,
        float distance,
        AnimationCurve curve,
        LayerMask obstacleLayer)
    {
        return RunAsync(direction, token, duration, distance, curve, obstacleLayer, MovementComponentExtension.CollisionMode.Stop);
    }

    public UniTask<bool> RollAsync(
        Vector3 direction,
        CancellationToken token,
        float duration,
        float distance,
        AnimationCurve curve,
        LayerMask obstacleLayer)
    {
        return RunAsync(direction, token, duration, distance, curve, obstacleLayer, MovementComponentExtension.CollisionMode.Slide);
    }

    private async UniTask<bool> RunAsync(
        Vector3 direction,
        CancellationToken token,
        float duration,
        float distance,
        AnimationCurve curve,
        LayerMask obstacleLayer,
        MovementComponentExtension.CollisionMode mode)
    {
        if (IsDashing) return false;

        IsDashing = true;

        try
        {
            return await MovementComponentExtension.MoveAsync(
                m_rb,
                m_collider,
                direction,
                token,
                duration,
                distance,
                curve,
                obstacleLayer,
                mode,
                m_groundCheck);
        }
        finally
        {
            IsDashing = false;
        }
    }
}