using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;

public class CardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;
    [SerializeField] private Button buttonAction;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textDescription;
    [SerializeField] private GameObject foreground;
    [SerializeField] private GameObject background;

    private SO_Card m_card;

    public event Action<CardUI, SO_Card> OnChosen;

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

        if (foreground) foreground.SetActive(false);
        if (background) background.SetActive(true);
        transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        transform.localScale = Vector3.one;

        SetInteractable(false);
    }

    public async UniTask FlipAsync(float duration)
    {
        bool swapped = false;

        await Tween.LocalEulerAngles(transform, startValue: new Vector3(0f, 180f, 0f), endValue: Vector3.zero, duration: duration, ease: Ease.InOutQuad)
            .OnUpdate(transform, (target, tween) =>
            {
                if (!swapped && target.localEulerAngles.y <= 90f)
                {
                    swapped = true;
                    if (foreground) foreground.SetActive(true);
                    if (background) background.SetActive(false);
                }
            });

        transform.localEulerAngles = Vector3.zero;
        if (foreground) foreground.SetActive(true);
        if (background) background.SetActive(false);

        SetInteractable(true);
    }

    public async UniTask PlaySelectAnimationAsync(float duration = 0.4f)
    {
        SetInteractable(false);

        await Tween.Scale(transform, endValue: Vector3.one * 1.15f, duration: duration * 0.3f, ease: Ease.OutQuad);

        await Tween.Scale(transform, endValue: Vector3.zero, duration: duration * 0.7f, ease: Ease.InQuad);
    }

    public void SetInteractable(bool value) => buttonAction.interactable = value;

    private void HandleClick() => OnChosen?.Invoke(this, m_card);
}