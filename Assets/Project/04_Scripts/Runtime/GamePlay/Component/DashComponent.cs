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

    public async UniTask<bool> DashAsync(
        Vector3 direction,
        CancellationToken token,
        float dashDuration,
        float dashDistance,
        AnimationCurve curve,
        LayerMask obstacleLayer)
    {
        if (IsDashing) return false;

        IsDashing = true;

        try
        {
            return await ForcedMovement.MoveAsync(
                m_rb,
                m_collider,
                direction,
                token,
                dashDuration,
                dashDistance,
                curve,
                obstacleLayer,
                m_groundCheck);
        }
        finally
        {
            IsDashing = false;
        }
    }
}