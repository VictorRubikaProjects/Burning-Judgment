using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;
    [SerializeField] private Button buttonAction;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textDescription;
    
    public void Setup(SO_Card card)
    {
        image.sprite = card.Icon;
        textName.text = card.Name;
        textDescription.text = card.Description;
    }
}