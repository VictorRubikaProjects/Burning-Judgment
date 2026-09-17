using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

/// <summary>
/// This enemy will be used to demonstrate the player capacity
/// </summary>
public class Dummy : AbstractEnemy
{
    [Header("References")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private MeshRenderer m_renderer;
    [SerializeField] private Material m_materialBody;

    [Header("Settings")] 
    [SerializeField] private Color m_colorNeutral = Color.red;
    [SerializeField] private Color m_colorHit = Color.yellow;
    [SerializeField] private Color m_colorReady = Color.green;
    
    private Material m_materialBodyInstance;
    private bool m_canTakeDamage = true;
    private CancellationTokenSource m_ctsDummy;

    protected override void Awake()
    {
        base.Awake();

        m_materialBodyInstance = new Material(m_materialBody);
        m_renderer.material = m_materialBodyInstance;
        
        ServiceLocator.Get<CameraService>().AddTarget(transform, 0.1f);
    }

    public override void TakeDamage()
    {
        base.TakeDamage();
        
        m_animator.Play("Hit");
        
        m_materialBodyInstance.color = m_colorHit;
        
        RecoverAsync().Forget();
    }

    public override bool CanTakeDamage() => m_canTakeDamage;

    private async UniTask RecoverAsync()
    {
        m_ctsDummy?.Cancel();
        m_ctsDummy =  new CancellationTokenSource();
        
        m_canTakeDamage = false;

        await Tween.MaterialColor(
            m_materialBodyInstance,
            m_colorHit,
            m_colorNeutral,
            3f,
            Ease.Linear);
        
        m_canTakeDamage = true;
        
        await RecoverFinishedAsync();
    }

    private async UniTask RecoverFinishedAsync()
    {
        m_ctsDummy.Token.ThrowIfCancellationRequested();
        
        await Tween.MaterialColor(m_materialBodyInstance,
            m_colorReady,
            m_colorNeutral,
            0.1f,
            Ease.Linear,
            5,
            CycleMode.Yoyo);
    }
}
