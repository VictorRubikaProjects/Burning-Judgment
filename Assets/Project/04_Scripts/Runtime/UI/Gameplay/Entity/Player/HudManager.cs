using System.Text;
using Event_Bus;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button[] optionButtons;
    
    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup optionCanvasGroup;
    
    [Header("Slider")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    
    [Header("Text")]
    [SerializeField] private TMP_Text levelIndicator;
    [SerializeField] private TMP_Text comboText;

    [Header("Combo Animation Settings")]
    [SerializeField] private float comboDisplayDuration = 1.5f;
    [SerializeField] private float comboHideDuration = 0.2f;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private Vector3 punchScaleStrength = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] private float minTiltAngle = -12f;
    [SerializeField] private float maxTiltAngle = 12f;
    [SerializeField] private int tiltCycles = 2;
    [SerializeField] private CycleMode tiltCycleMode = CycleMode.Yoyo;
    [SerializeField] private Ease animationEase = Ease.Default;

    [Header("Combo Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color tier1Color = Color.yellow; 
    [SerializeField] private Color tier2Color = new Color(1f, 0.5f, 0f); 
    [SerializeField] private Color tier3Color = Color.red; 
    
    private OptionController m_optionController;
    private Sequence m_comboSequence;
    private EventBinding<PlayerComboEvent> m_eventBindingCombo;
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
        EventBus<PlayerComboEvent>.Register(m_eventBindingCombo);
    }

    private void OnDestroy()
    {
        m_comboSequence.Stop();
        m_optionController.Clean();
        EventBus<PlayerComboEvent>.Unregister(m_eventBindingCombo);
    }

    private void OnShowCombo(PlayerComboEvent e)
    {
        m_comboSequence.Stop();
        
        comboText.rectTransform.localScale = Vector3.one;
        comboText.rectTransform.localEulerAngles = Vector3.zero;
        
        m_sbCombo.Clear();
        m_sbCombo.Append("Combo ");
        m_sbCombo.Append(e.Combo);
        comboText.text = m_sbCombo.ToString();

        comboText.color = GetComboColor(e.Combo);

        float randomTilt = Random.Range(minTiltAngle, maxTiltAngle);
        
        m_comboSequence = Sequence.Create()
            .Group(Tween.PunchScale(comboText.rectTransform, punchScaleStrength, punchDuration))
            .Group(Tween.LocalEulerAngles(comboText.rectTransform, Vector3.zero, new Vector3(0f, 0f, randomTilt), punchDuration, animationEase, tiltCycles, tiltCycleMode))
            .Chain(Tween.Delay(comboDisplayDuration))
            .Chain(Tween.Scale(comboText.rectTransform, Vector3.zero, comboHideDuration));
    }

    private Color GetComboColor(int comboCount)
    {
        return comboCount switch
        {
            >= 15 => tier3Color,
            >= 7 => tier2Color,
            >= 3 => tier1Color,
            _ => defaultColor
        };
    }
}