using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

public class PlayerAspectComponent : ActorComponent
{
    private readonly Color m_playerColorNeutral = Color.white;
    private readonly Color m_playerColorAttackReady = Color.green;
    private readonly Color m_playerColorHurt = Color.red;

    private readonly ConfigStatsPlayer m_statsPlayer;
    private readonly MeshRenderer m_playerRenderer;
    private Material m_playerMaterialInstance;

    private Tween m_currentTween;

    public PlayerAspectComponent(Actor owner, Material playerMaterial, MeshRenderer playerRenderer, ConfigStatsPlayer stats) : base(owner)
    {
        m_playerMaterialInstance = new Material(playerMaterial);
        m_playerRenderer = playerRenderer;
        m_playerRenderer.material = m_playerMaterialInstance;
        m_statsPlayer = stats;
    }

    public void AttackReadyVisuals() => AttackReadyVisualsAsync().Forget();

    public void CancelAttackReadyVisuals()
    {
        m_currentTween.Stop();
        m_playerMaterialInstance.color = m_playerColorNeutral;
    }

    public void HurtVisuals() => HurtVisualsAsync().Forget();

    public void CancelHurtVisuals()
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

    private async UniTaskVoid HurtVisualsAsync()
    {
        m_currentTween.Stop();

        int cycle = 3;
        float duration = 0.08f;

        m_currentTween = Tween.MaterialColor(m_playerMaterialInstance,
            m_playerColorNeutral,
            m_playerColorHurt,
            duration,
            Ease.Linear,
            cycle,
            CycleMode.Yoyo);

        await m_currentTween;

        m_playerMaterialInstance.color = m_playerColorNeutral;
    }
}