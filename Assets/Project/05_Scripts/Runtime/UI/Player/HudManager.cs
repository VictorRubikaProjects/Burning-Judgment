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
    
    private OptionController m_optionController;
    
    private void Awake()
    {
        m_optionController = new OptionController(
            optionButtons,
            optionCanvasGroup,
            volumeSlider,
            sfxSlider,
            musicSlider);
        
        m_optionController.Init();
    }
    
    
}
