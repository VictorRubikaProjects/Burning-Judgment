using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

public class PlayerAspectComponent : ActorComponent
{
    private Material m_playerMaterialInstance;
    private MeshRenderer m_playerRenderer;
    
    private Color m_playerColorNeutral = Color.white;
    private Color m_playerColorAttackReady = Color.green;

    private Tween m_currentTween;
    
    private ConfigStatsPlayer m_statsPlayer;
    
    public PlayerAspectComponent(Actor owner, Material playerMaterial, MeshRenderer playerRenderer, ConfigStatsPlayer stats) : base(owner)
    {
        m_playerMaterialInstance = new Material(playerMaterial);
        m_playerRenderer = playerRenderer;
        m_playerRenderer.material = m_playerMaterialInstance;
        m_statsPlayer = stats;
    }

    public void AttackReadyVisuals() => AttackReadyVisualsAsync().Forget();

    private async UniTaskVoid AttackReadyVisualsAsync()
    {
        m_currentTween.Stop();

        int cycle = 5;
        float duration = (m_statsPlayer.TimerAttack + m_statsPlayer.ThresholdTimerAttack) / cycle;

        m_currentTween = Tween.MaterialColor(m_playerMaterialInstance,
            m_playerColorNeutral,
            m_playerColorAttackReady,
            duration,
            Ease.Linear,
            cycle,
            CycleMode.Yoyo);

        await m_currentTween;
        
        m_playerMaterialInstance.color = m_playerColorNeutral;
    }
    
    public void CancelAttackReadyVisuals()
    {
        m_currentTween.Stop();
        m_playerMaterialInstance.color = m_playerColorNeutral;
    }

    public override void Dispose()
    {
        base.Dispose();
        m_currentTween.Stop();
        Object.Destroy(m_playerMaterialInstance);
        m_playerMaterialInstance = null;
    }
}
