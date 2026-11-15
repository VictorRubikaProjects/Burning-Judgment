using System;
using System.Text;
using Event_Bus;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [System.Serializable]
    public struct ComboAnimationConfig
    {
        [Header("Timing")]
        public float displayDuration;
        public float hideDuration;
        public float punchDuration;

        [Header("Effects")]
        public Vector3 punchScaleStrength;
        public float minTiltAngle;
        public float maxTiltAngle;
        public int tiltCycles;
        public CycleMode tiltCycleMode;
        public Ease animationEase;
    }

    [System.Serializable]
    public struct ComboColorConfig
    {
        public Color defaultColor;
        public Color tier1Color; 
        public Color tier2Color; 
        public Color tier3Color; 
    }

    [Header("Configuration Asset")]
    [SerializeField] private SO_HudConfig hudConfig;

    [Header("UI Controllers & Components")]
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private Sprite[] spritesIndicatorStabilityPlayer;
    
    [SerializeField] private CanvasGroup optionCanvasGroup;
    
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    
    [SerializeField] private TMP_Text levelIndicator;
    [SerializeField] private TMP_Text comboText;
    
    [SerializeField] private Image stabilityImage;
    
    private OptionController m_optionController;
    
    private Sequence m_comboSequence;
    private Sequence m_stabilitySequence; 
    
    private EventBinding<PlayerComboEvent> m_eventBindingCombo;
    private EventBinding<PlayerStabilityUpdate> m_eventBindingStability;
    
    private readonly StringBuilder m_sbCombo = new(20);
    
    private void Awake()
    {
        m_optionController = new OptionController(
            optionButtons,
            optionCanvasGroup,
            volumeSlider,
            sfxSlider,
            musicSlider);
        
        m_optionController.Init();

        levelIndicator.text = ServiceLocator.Get<GameService>().CurrentFloor.ToString();
        comboText.rectTransform.localScale = Vector3.zero;
        
        m_eventBindingCombo = new EventBinding<PlayerComboEvent>(OnShowCombo);
        m_eventBindingStability  = new EventBinding<PlayerStabilityUpdate>(OnStabilityChanged);
        
        EventBus<PlayerComboEvent>.Register(m_eventBindingCombo);
        EventBus<PlayerStabilityUpdate>.Register(m_eventBindingStability);
    }

    private void OnDestroy()
    {
        m_comboSequence.Stop();
        m_stabilitySequence.Stop();
        m_optionController.Clean();
        EventBus<PlayerComboEvent>.Unregister(m_eventBindingCombo);
        EventBus<PlayerStabilityUpdate>.Unregister(m_eventBindingStability);
    }

    private void OnShowCombo(PlayerComboEvent e)
    {
        m_comboSequence.Stop();
        
        var rectTransform = comboText.rectTransform;
        rectTransform.localScale = Vector3.one;
        rectTransform.localEulerAngles = Vector3.zero;
        
        m_sbCombo.Clear().Append("Combo ").Append(e.Combo);
        comboText.text = m_sbCombo.ToString();
        comboText.color = GetComboColor(e.Combo);

        var animConfig = hudConfig.comboAnimation;
        float randomTilt = UnityEngine.Random.Range(animConfig.minTiltAngle, animConfig.maxTiltAngle);
        
        m_comboSequence = Sequence.Create()
            .Group(Tween.PunchScale(rectTransform, animConfig.punchScaleStrength, animConfig.punchDuration))
            .Group(Tween.LocalEulerAngles(rectTransform, Vector3.zero, new Vector3(0f, 0f, randomTilt), animConfig.punchDuration, animConfig.animationEase, animConfig.tiltCycles, animConfig.tiltCycleMode))
            .Chain(Tween.Delay(animConfig.displayDuration))
            .Chain(Tween.Scale(rectTransform, Vector3.zero, animConfig.hideDuration));
    }

    private Color GetComboColor(int comboCount)
    {
        var colors = hudConfig.comboColors;
        return comboCount switch
        {
            >= 15 => colors.tier3Color,
            >= 7 => colors.tier2Color,
            >= 3 => colors.tier1Color,
            _ => colors.defaultColor
        };
    }

    private void OnStabilityChanged(PlayerStabilityUpdate e)
    {
        m_stabilitySequence.Stop();
        
        stabilityImage.sprite = GetSpriteByStability(e.Stability);
        
        var rectTransform = stabilityImage.rectTransform;
        rectTransform.localScale = Vector3.one;
        rectTransform.localEulerAngles = Vector3.zero;
        
        float randomTilt = UnityEngine.Random.Range(hudConfig.stabilityMinTilt, hudConfig.stabilityMaxTilt);
        var animConfig = hudConfig.comboAnimation; 
        
        m_stabilitySequence = Sequence.Create()
            .Group(Tween.PunchScale(rectTransform, hudConfig.stabilityPunchScale, hudConfig.stabilityPunchDuration))
            .Group(Tween.LocalEulerAngles(rectTransform, Vector3.zero, new Vector3(0f, 0f, randomTilt), hudConfig.stabilityPunchDuration, animConfig.animationEase, animConfig.tiltCycles, animConfig.tiltCycleMode));
    }
    
    private Sprite GetSpriteByStability(float eStability)
    {
        return eStability switch
        {
            <= 0.2f => spritesIndicatorStabilityPlayer[3],
            <= 0.6f => spritesIndicatorStabilityPlayer[2],
            <= 0.99f => spritesIndicatorStabilityPlayer[1],
            _ => spritesIndicatorStabilityPlayer[0]
        };
    }
}