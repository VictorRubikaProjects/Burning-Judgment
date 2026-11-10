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

    [Header("Combo Configuration")]
    [SerializeField] private ComboAnimationConfig comboAnimation = new()
    {
        displayDuration = 1.5f,
        hideDuration = 0.2f,
        punchDuration = 0.25f,
        punchScaleStrength = new Vector3(0.3f, 0.3f, 0.3f),
        minTiltAngle = -12f,
        maxTiltAngle = 12f,
        tiltCycles = 2,
        tiltCycleMode = CycleMode.Yoyo,
        animationEase = Ease.Default
    };

    [SerializeField] private ComboColorConfig comboColors = new()
    {
        defaultColor = Color.white,
        tier1Color = Color.yellow,
        tier2Color = new Color(1f, 0.5f, 0f),
        tier3Color = Color.red
    };
    
    private OptionController m_optionController;
    
    private Sequence m_comboSequence;
    
    private Tween m_stabilityTween;
    
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
        m_stabilityTween.Stop();
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
        
        m_sbCombo.Clear().Append("Combo ").Append($"{e.Multiplier:0.00}");
        comboText.text = m_sbCombo.ToString();
        comboText.color = GetComboColor(e.Multiplier);

        float randomTilt = UnityEngine.Random.Range(comboAnimation.minTiltAngle, comboAnimation.maxTiltAngle);
        
        m_comboSequence = Sequence.Create()
            .Group(Tween.PunchScale(rectTransform, comboAnimation.punchScaleStrength, comboAnimation.punchDuration))
            .Group(Tween.LocalEulerAngles(rectTransform, Vector3.zero, new Vector3(0f, 0f, randomTilt), comboAnimation.punchDuration, comboAnimation.animationEase, comboAnimation.tiltCycles, comboAnimation.tiltCycleMode))
            .Chain(Tween.Delay(comboAnimation.displayDuration))
            .Chain(Tween.Scale(rectTransform, Vector3.zero, comboAnimation.hideDuration));
    }

    private Color GetComboColor(float comboCount)
    {
        return comboCount switch
        {
            >= 15 => comboColors.tier3Color,
            >= 7 => comboColors.tier2Color,
            >= 3 => comboColors.tier1Color,
            _ => comboColors.defaultColor
        };
    }

    private void OnStabilityChanged(PlayerStabilityUpdate e)
    {
        m_stabilityTween.Stop();
        
        stabilityImage.sprite = GetSpriteByStability(e.Stability);
        
        float randomTilt = UnityEngine.Random.Range(10, 20);
        
        m_stabilityTween = Tween.LocalEulerAngles(
            stabilityImage.rectTransform,
            Vector3.zero,
            new Vector3(0f, 0f, randomTilt),
            0.1f,
            comboAnimation.animationEase,
            comboAnimation.tiltCycles,
            comboAnimation.tiltCycleMode);
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