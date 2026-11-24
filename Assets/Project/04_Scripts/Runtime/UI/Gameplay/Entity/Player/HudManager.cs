using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Event_Bus;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [Header("Configuration Asset")]
    [SerializeField] private SO_HudConfig hudConfig;

    [Header("UI Controllers & Components")]
    [SerializeField] private Button[] optionButtons;

    [SerializeField] private CanvasGroup optionCanvasGroup;
    [SerializeField] private CanvasGroup floorIndicatorCanvasGroup;

    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text floorText;

    private OptionController m_optionController;

    private Sequence m_comboSequence;

    private EventBinding<PlayerComboEvent> m_eventBindingCombo;
    private EventBinding<RoundAnnouncement> m_eventBindingRoundAnnouncement;

    private readonly StringBuilder m_sbCombo = new(20);

    private void Awake()
    {
        m_optionController = new OptionController(
            optionButtons,
            optionCanvasGroup,
            sfxSlider,
            musicSlider);

        m_optionController.Init();

        comboText.rectTransform.localScale = Vector3.zero;

        floorText.text = $"FLOOR {ServiceLocator.Get<GameService>().CurrentFloor}";
        floorIndicatorCanvasGroup.alpha = 0;

        m_eventBindingCombo = new EventBinding<PlayerComboEvent>(OnShowCombo);
        m_eventBindingRoundAnnouncement = new EventBinding<RoundAnnouncement>(OnRoundAnnouncement);

        EventBus<PlayerComboEvent>.Register(m_eventBindingCombo);
        EventBus<RoundAnnouncement>.Register(m_eventBindingRoundAnnouncement);
    }

    private void OnDestroy()
    {
        m_comboSequence.Stop();
        m_optionController.Clean();
        EventBus<PlayerComboEvent>.Unregister(m_eventBindingCombo);
        EventBus<RoundAnnouncement>.Unregister(m_eventBindingRoundAnnouncement);
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

        float scaleMultiplier = 1f + (e.Combo * hudConfig.comboScaleGrowthRate);
        scaleMultiplier = Mathf.Min(scaleMultiplier, hudConfig.maxComboScaleMultiplier);
        Vector3 dynamicPunchScale = animConfig.punchScaleStrength * scaleMultiplier;

        m_comboSequence = Sequence.Create()
            .Group(Tween.PunchScale(rectTransform, dynamicPunchScale, animConfig.punchDuration))
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

    private async UniTask FadeFloorIndicator(float duration)
    {
        duration /= 3;
        await Tween.Alpha(floorIndicatorCanvasGroup, endValue: 1f, duration: duration, ease: Ease.InOutCirc);
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        await Tween.Alpha(floorIndicatorCanvasGroup, endValue: 0f, duration: duration, ease: Ease.InOutCirc);
    }

    private void OnRoundAnnouncement(RoundAnnouncement e) => FadeFloorIndicator(e.Duration).Forget();
}