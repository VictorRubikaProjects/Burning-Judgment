using System;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransitionService : IGameService
{
    #region Variables

    private GameObject m_loadingScreenGo;
    private GameObject m_transitionCircle;
    private GameObject m_floorLabelGo;

    private TMP_Text m_floorLabelText; 

    private Material m_transitionMaterialInstance;

    private const float m_transitionCircleFadeInValue = 0;
    private const float m_transitionCircleFadeOutValue = 1;

    private int m_transitionShaderParam = Shader.PropertyToID("_Progress");

    private const float m_circleDuration = 0.5f;
    private const float m_floorHoldDuration = 1.5f;
    private const float m_floorFadeDuration = 0.3f;

    #endregion

    #region IGameService Core

    public bool IsInitialized { get; set; }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(m_transitionMaterialInstance);
    }

    public UniTask InitializeService()
    {
        m_loadingScreenGo = GameObject.Find("PF_LoadingScreen");
        m_transitionCircle = GameObject.Find("PF_Transition_Circle");
        m_floorLabelGo = GameObject.Find("PF_FloorLabel");

        if (m_loadingScreenGo == null)
        {
            Debug.LogError("[TransitionService] LoadingScreen object not found.");
        }

        if (m_transitionCircle == null)
        {
            Debug.LogError("[TransitionService] Circle Transition object not found.");
        }

        if (m_floorLabelGo == null)
        {
            Debug.LogError("[TransitionService] FloorLabel object not found.");
        }

        Image transitionCircle = m_transitionCircle.GetComponent<Image>();
        
        Material transitionMaterial = transitionCircle.material;
        
        m_transitionMaterialInstance = new Material(transitionMaterial);
        
        transitionCircle.material = m_transitionMaterialInstance;
        
        m_transitionMaterialInstance.SetFloat(m_transitionShaderParam, m_transitionCircleFadeOutValue);

        m_floorLabelText = m_floorLabelGo.GetComponentInChildren<TMP_Text>();
        
        m_floorLabelGo.SetActive(false);

        return UniTask.CompletedTask;
    }

    public void ShutDownService() { }

    public void Tick() { }

    #endregion

    #region Loading Screen

    public void ToggleLoadingScreen(bool on)
    {
        if (!m_loadingScreenGo) throw new ArgumentNullException(nameof(on), "[TransitionService.ToggleLoadingScreen] LoadingScreen object not found.");
        
        m_loadingScreenGo.SetActive(on);
    }

    #endregion

    #region Circle Transition

    public async UniTask CircleTransitionFadeIn() => await CircleTransition(m_transitionCircleFadeInValue);
    public async UniTask CircleTransitionFadeOut() => await CircleTransition(m_transitionCircleFadeOutValue);

    private async UniTask CircleTransition(float target)
    {
        await Tween.MaterialProperty(
            m_transitionMaterialInstance,
            m_transitionShaderParam,
            target,
            m_circleDuration,
            Ease.InOutQuad);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }

    #endregion

    #region Floor Announcement

    public async UniTask ShowFloorAsync(int floor)
    {
        int oldFloor = floor - 1;
        
        m_floorLabelText.text = oldFloor.ToString();
        
        m_floorLabelGo.SetActive(true);

        CanvasGroup canvasGroup = m_floorLabelGo.GetComponent<CanvasGroup>();

        await Tween.Custom(0f, 1f, m_floorFadeDuration, v => canvasGroup.alpha = v, Ease.InOutQuad);
        
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        
        await Tween.LocalRotation(m_floorLabelText.rectTransform, new TweenSettings<Vector3>(new Vector3(0,180,0),0.2f));
        
        m_floorLabelText.text = floor.ToString();
        
        await Tween.LocalRotation(m_floorLabelText.rectTransform, new TweenSettings<Vector3>(new Vector3(0,360,0),0.2f));
        
        await UniTask.Delay(TimeSpan.FromSeconds(m_floorHoldDuration));
        
        await Tween.Custom(1f, 0f, m_floorFadeDuration, v => canvasGroup.alpha = v, Ease.InOutQuad);

        m_floorLabelGo.SetActive(false);
    }

    #endregion
}