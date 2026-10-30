using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;

public class TricksController : MonoBehaviour
{
    [Header("References Data")]
    [SerializeField] private SO_Tricks soTricks;

    [Header("UI References")]
    [SerializeField] private TMP_Text trickTextField;
    [SerializeField] private Button nextButton;             
    [SerializeField] private Button prevButton;             

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.25f;     
    [SerializeField] private float buttonPunchScale = 0.15f;  

    private int currentIndex;
    private bool isAnimating;

    private void Start()
    {
        if (soTricks == null || soTricks.tricksList == null || soTricks.tricksList.Length == 0)
        {
            Debug.LogWarning("Aucun SO_Tricks assigné ou la liste d'astuces est vide.", this);
            return;
        }

        nextButton.onClick.AddListener(OnNextClicked);
        prevButton.onClick.AddListener(OnPreviousClicked);

        ShowTrickInstant(currentIndex);
    }

    private void OnDestroy()
    {
        if (nextButton != null) nextButton.onClick.RemoveListener(OnNextClicked);
        if (prevButton != null) prevButton.onClick.RemoveListener(OnPreviousClicked);
    }

    private void OnNextClicked()
    {
        if (isAnimating) return;
        AnimateButton(nextButton.transform);

        currentIndex = (currentIndex + 1) % soTricks.tricksList.Length;
        ChangeTrickWithFade();
    }

    private void OnPreviousClicked()
    {
        if (isAnimating) return;
        AnimateButton(prevButton.transform);

        currentIndex = (currentIndex - 1 + soTricks.tricksList.Length) % soTricks.tricksList.Length;
        ChangeTrickWithFade();
    }

    private void ChangeTrickWithFade()
    {
        isAnimating = true;

        Tween.Alpha(trickTextField, endValue: 0f, duration: fadeDuration)
            .OnComplete(() =>
            {
                trickTextField.text = soTricks.tricksList[currentIndex];

                Tween.Alpha(trickTextField, endValue: 1f, duration: fadeDuration)
                    .OnComplete(() => { isAnimating = false; });
            });
    }

    private void ShowTrickInstant(int index)
    {
        trickTextField.text = soTricks.tricksList[index];
        Color c = trickTextField.color;
        c.a = 1f;
        trickTextField.color = c;
    }

    private void AnimateButton(Transform buttonTransform)
    {
        Tween.StopAll(buttonTransform);
        Tween.PunchScale(buttonTransform, strength: Vector3.one * buttonPunchScale, duration: 0.2f);
    }
}