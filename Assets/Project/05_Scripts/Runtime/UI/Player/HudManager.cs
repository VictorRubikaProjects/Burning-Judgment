using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private CanvasGroup optionPanel;
    
    private OptionController m_optionController;
    
    private void Awake()
    {
        m_optionController = new OptionController(optionButtons, optionPanel);
        m_optionController.Init();
    }
    
    
}
