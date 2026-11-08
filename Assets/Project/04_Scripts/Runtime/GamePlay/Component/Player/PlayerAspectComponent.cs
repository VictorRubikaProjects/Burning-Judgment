using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

public class PlayerAspectComponent : ActorComponent
{
    private readonly SO_PlayerStats m_stats;
    
    private readonly SkinnedMeshRenderer m_playerRenderer;
    private CanvasGroup m_slowMotionCanvasGroup;
    
    private Material m_playerMaterialInstance;

    private Tween m_currentTween;
    private Tween m_slowMotionTween;

    public PlayerAspectComponent(Actor owner, Material playerMaterial, SkinnedMeshRenderer playerRenderer, SO_PlayerStats stats, CanvasGroup slowMotionCanvasGroup) : base(owner)
    {
        m_playerMaterialInstance = new Material(playerMaterial);
        
        m_playerRenderer = playerRenderer;
        
        m_playerRenderer.material = m_playerMaterialInstance;
        
        m_stats = stats;

        m_slowMotionCanvasGroup = slowMotionCanvasGroup;
        m_slowMotionCanvasGroup.alpha = 0f;
    }
    
    public void HurtVisuals() => HurtVisualsAsync().Forget();

    public override void Dispose()
    {
        base.Dispose();
        m_currentTween.Stop();
        m_slowMotionTween.Stop();
        Object.Destroy(m_playerMaterialInstance);
        m_playerMaterialInstance = null;
    }
    
    private async UniTaskVoid HurtVisualsAsync()
    {
        m_currentTween.Stop();

        int cycle = 3;
        float duration = 0.08f;

        for (int i = 0; i < cycle * 2; i++)
        {
            m_playerRenderer.enabled = !m_playerRenderer.enabled;
            await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        }

        m_playerRenderer.enabled = true;
    }

    public async UniTaskVoid CancelSlowMotionVisualsAsync()
    {
        m_slowMotionTween.Stop();
        m_slowMotionTween = Tween.Alpha(m_slowMotionCanvasGroup, 0f, 0.4f);
    }
    
    public async UniTaskVoid StartSlowMotionVisualsAsync()
    {
        m_slowMotionTween.Stop();
        m_slowMotionTween = Tween.Alpha(m_slowMotionCanvasGroup, 1f, 0.4f);
    }
}