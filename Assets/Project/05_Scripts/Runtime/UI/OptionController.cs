using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class OptionController
{
    private Button[] m_optionButtons;
    
    private CanvasGroup m_optionPanel;
    
    private bool m_isOpen;
    private bool m_pauseTime;

    private float m_durationOptionTween = 0.3f;
    
    Tween m_optionTween;

    public OptionController(Button[] optionButtons, CanvasGroup optionPanel, bool pauseTime = true, float startingAlpha = 0)
    {
        m_optionButtons =  optionButtons;
        m_optionPanel = optionPanel;
        m_pauseTime = pauseTime;
        m_optionPanel.alpha = startingAlpha;
        if (startingAlpha == 0) m_optionPanel.blocksRaycasts = false;
    }

    public void Init()
    {
        foreach (Button button in m_optionButtons)
            button.onClick.AddListener(ToggleOption);
    }

    private void ToggleOption()
    {
        if (m_optionTween.isAlive)
            m_optionTween.Stop();
        
        m_isOpen = !m_isOpen;
        
        float targetAlpha = m_isOpen ? 1f : 0f;
        
        m_optionPanel.blocksRaycasts = m_isOpen;
        
        if (m_pauseTime) Time.timeScale = m_isOpen ? 0 : 1;
        
        m_optionTween = Tween.Alpha(m_optionPanel,targetAlpha,m_durationOptionTween,Ease.Linear,useUnscaledTime:true);
    }
}
