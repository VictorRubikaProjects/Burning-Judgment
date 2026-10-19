using System;
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

    private SO_Card m_card;

    public event Action<SO_Card> OnChosen;

    private void Awake()
    {
        buttonAction.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        buttonAction.onClick.RemoveListener(HandleClick);
    }

    public void Setup(SO_Card card)
    {
        m_card = card;

        image.sprite = card.Icon;
        image.enabled = card.Icon != null;
        textName.text = card.DisplayName;
        textDescription.text = card.Description;

        SetInteractable(false);
    }

    public void SetInteractable(bool value) => buttonAction.interactable = value;

    private void HandleClick() => OnChosen?.Invoke(m_card);
}