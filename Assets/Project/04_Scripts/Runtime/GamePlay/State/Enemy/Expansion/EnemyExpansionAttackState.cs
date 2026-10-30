using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using PrimeTween;
using UnityEngine;

public class EnemyExpansionAttackState : EnemyBaseState
{
    public bool IsFinished { get; private set; } = false;

    private readonly EnemyExpansion m_expansion;
    private readonly Collider[] m_strikeHits = new Collider[4];

    private CancellationTokenSource m_cts;
    private CameraService m_cameraService;
    private Tween m_chargeTween;

    private static readonly int m_idPropMaterialProgressionHash = Shader.PropertyToID("_Progression");

    public EnemyExpansionAttackState(EnemyExpansion owner) : base(owner)
    {
        m_expansion = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        IsFinished = false;

        m_cts = new CancellationTokenSource();
        AttackAsync(m_cts.Token).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();

        m_cts?.Cancel();
        m_cts?.Dispose();
        m_cts = null;

        if (m_chargeTween.isAlive) m_chargeTween.Stop();

        m_expansion.MaterialChargeAttackInstance.SetFloat(m_idPropMaterialProgressionHash, 0f);
    }

    private async UniTask AttackAsync(CancellationToken token)
    {
        float duration = m_expansion.Stats.ChargeDuration;

        StartChargeFeedback(duration);

        bool canceled = await UniTask
            .Delay(TimeSpan.FromSeconds(duration), cancellationToken: token)
            .SuppressCancellationThrow();

        if (canceled) return;

        ExecuteStrike();
        IsFinished = true;
    }

    private void ExecuteStrike()
    {
        m_expansion.ConsumeAttackRequest();
        
        Vector3 center = m_expansion.TransformCache.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            m_expansion.Stats.AttackRadius,
            m_strikeHits,
            m_expansion.Stats.PlayerLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            PlayerCharacter player = m_strikeHits[i].GetComponentInParent<PlayerCharacter>();
            if (!player) continue;

            Vector3 offset = player.transform.position - center;
            offset.y = 0f;
            float distance = offset.magnitude;

            Vector3 direction = distance > 0.01f
                ? offset / distance
                : m_expansion.TransformCache.forward;

            float multiplier = m_expansion.Stats.GetFalloffMultiplier(distance);

            EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(
                player,
                direction,
                m_expansion.Stats.PushForce * multiplier,
                m_expansion.Stats.Destabilization));

            m_cameraService ??= ServiceLocator.Get<CameraService>();
            m_cameraService.Shake();

            return;
        }
    }

    private void StartChargeFeedback(float duration)
    {
        m_chargeTween = Tween.MaterialProperty(
            m_expansion.MaterialChargeAttackInstance,
            m_idPropMaterialProgressionHash,
            1f,
            duration);
    }
}