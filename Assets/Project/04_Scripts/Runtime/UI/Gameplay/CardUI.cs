using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using PrimeTween;

public class CardUI : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private CanvasGroup alphaCard;
    [SerializeField] private Image imageIcon;
    [SerializeField] private Button buttonAction;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textDescription;
    [SerializeField] private GameObject foreground;
    [SerializeField] private GameObject background;

    [Header("Theming / Rarity Elements")]
    [SerializeField] private Image imageCardBackground;
    [SerializeField] private Image imageForeGround;        

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

    public void Setup(SO_Card card, SO_CardThemeDatabase themeDatabase)
    {
        m_card = card;

        imageIcon.sprite = card.Icon;
        imageIcon.enabled = card.Icon != null;
        textName.text = card.DisplayName;
        textDescription.text = card.Description;

        if (themeDatabase != null)
        {
            SO_CardRarityTheme theme = themeDatabase.GetTheme(card.Rarity);
            if (theme != null)
            {
                textName.color = theme.TitleTextColor;
                textDescription.color = theme.DescriptionTextColor;

                if (imageCardBackground && theme.BackgroundSprite) 
                    imageCardBackground.sprite = theme.BackgroundSprite;
                
                if (imageForeGround && theme.ForegroundSprite) 
                    imageForeGround.sprite = theme.ForegroundSprite;
            }
        }

        if (foreground) foreground.SetActive(false);
        if (background) background.SetActive(true);
        transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        transform.localScale = Vector3.zero;

        SetInteractable(false);
    }

    public async UniTask FlipAsync(float duration, CancellationTokenSource cts, int index)
    {
        duration *= (index + 1);
        
        await Tween.Scale(transform, Vector3.one, duration, ease: Ease.InOutElastic);
        
        bool swapped = false;
        
        await UniTask.WaitUntilCanceled(cts.Token);

        duration *= 1.2f;
        
        await Tween.LocalEulerAngles(transform, startValue: new Vector3(0f, 180f, 0f), endValue: Vector3.zero, duration: duration / 2, ease: Ease.InOutQuad)
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

    public async UniTask PlayNonSelectAnimationAsync(float duration = 0.4f)
    {
        SetInteractable(false);
        //await Tween.Scale(transform, endValue: Vector3.zero, duration);
        //I prefer the upper style but Bove do not sadly what an idiot
        await Tween.Alpha(alphaCard, endValue: 0f, duration, Ease.OutCirc);
    }

    public void SetInteractable(bool value) => buttonAction.interactable = value;

    private void HandleClick() => OnChosen?.Invoke(this, m_card);
}