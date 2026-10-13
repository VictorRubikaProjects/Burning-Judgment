using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class EnemyExpansionAttackState : EnemyBaseState
{
    public bool IsFinished {get; private set;} = false;
    
    private readonly EnemyExpansion m_expansion;
    
    private readonly Collider[] m_strikeHits = new Collider[4];
    
    private CancellationTokenSource m_cts;
    
    private CameraService m_cameraService;
    
    public EnemyExpansionAttackState(EnemyExpansion owner) : base(owner)
    {
        m_expansion = owner;
        m_cameraService = ServiceLocator.Get<CameraService>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
    }

    public override void OnExit()
    {
        base.OnExit();
        m_expansion.ConsumeAttackRequest();
    }

    private async UniTask AttackAsync()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(m_expansion.Stats.TimerChargeAttack));
        
        ExecuteStrike();
        
        IsFinished = true;
    }
    
    private void ExecuteStrike()
    {
        Transform dasherTransform = m_expansion.TransformCache;
        Vector3 center = dasherTransform.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            m_expansion.Stats.RadiusAttack,
            m_strikeHits,
            m_expansion.Stats.PlayerLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            PlayerCharacter player = m_strikeHits[i].GetComponentInParent<PlayerCharacter>();
            if (!player) continue;

            EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(
                player,
                Owner.TransformCache.forward,
                m_expansion.Stats.Force));
            
            m_cameraService.Shake();
            
            return;
        }
    }
}