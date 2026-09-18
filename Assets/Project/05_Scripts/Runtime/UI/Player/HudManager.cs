using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private GameObject optionPanel;
    
    private bool m_isOpen;

    private void Awake()
    {
        foreach (Button button in optionButtons)
        {
            button.onClick.AddListener(ToggleOption);
        }
    }
    
    private void ToggleOption()
    {
        m_isOpen = !m_isOpen;
        optionPanel.SetActive(m_isOpen);
        Time.timeScale = m_isOpen ? 0 : 1;
    }
}
