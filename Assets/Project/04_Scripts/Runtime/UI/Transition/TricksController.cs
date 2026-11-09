using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using System.Threading;
using Cysharp.Threading.Tasks;

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

    private Tween m_fadeTween;
    private CancellationTokenSource m_cts;
    
    private AudioService m_audioService;

    private void Awake()
    {
        m_cts = new CancellationTokenSource();
        m_audioService = ServiceLocator.Get<AudioService>();
    }

    private void Start()
    {
        if (!ValidateSetup()) return;

        if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
        if (prevButton != null) prevButton.onClick.AddListener(OnPreviousClicked);

        ShowTrickInstant(currentIndex);
    }

    private void OnDestroy()
    {
        m_cts?.Cancel();
        m_cts?.Dispose();

        if (nextButton != null) nextButton.onClick.RemoveListener(OnNextClicked);
        if (prevButton != null) prevButton.onClick.RemoveListener(OnPreviousClicked);
        
        m_fadeTween.Stop();
    }

    private bool ValidateSetup()
    {
        if (soTricks == null || soTricks.tricksList == null || soTricks.tricksList.Length == 0)
        {
            Debug.LogWarning("[TricksController] Aucun SO_Tricks assigné ou la liste d'astuces est vide.", this);
            gameObject.SetActive(false);
            return false;
        }

        if (trickTextField == null || nextButton == null || prevButton == null)
        {
            Debug.LogError("[TricksController] Des références UI (Text ou Boutons) sont manquantes dans l'inspecteur.", this);
            gameObject.SetActive(false);
            return false;
        }

        return true;
    }

    private void OnNextClicked()
    {
        if (isAnimating || soTricks == null || soTricks.tricksList.Length == 0) return;
        AnimateButton(nextButton.transform);

        currentIndex = (currentIndex + 1) % soTricks.tricksList.Length;
        ChangeTrickWithFadeAsync().Forget();
        m_audioService.PlaySfx(m_audioService.Bank.UIButtonEvent);
    }

    private void OnPreviousClicked()
    {
        if (isAnimating || soTricks == null || soTricks.tricksList.Length == 0) return;
        AnimateButton(prevButton.transform);

        currentIndex = (currentIndex - 1 + soTricks.tricksList.Length) % soTricks.tricksList.Length;
        ChangeTrickWithFadeAsync().Forget();
        m_audioService.PlaySfx(m_audioService.Bank.UIButtonEvent);
    }

    private async UniTaskVoid ChangeTrickWithFadeAsync()
    {
        m_fadeTween.Stop();
        isAnimating = true;

        try
        {
            m_fadeTween = Tween.Alpha(trickTextField, endValue: 0f, duration: fadeDuration);
            await m_fadeTween;

            if (trickTextField == null || soTricks == null) return;

            trickTextField.text = soTricks.tricksList[currentIndex];

            m_fadeTween = Tween.Alpha(trickTextField, endValue: 1f, duration: fadeDuration);
            await m_fadeTween;
        }
        catch (System.OperationCanceledException)
        {
            
        }
        finally
        {
            isAnimating = false;
        }
    }

    private void ShowTrickInstant(int index)
    {
        if (trickTextField == null || soTricks == null || soTricks.tricksList.Length == 0) return;

        trickTextField.text = soTricks.tricksList[index];
        Color c = trickTextField.color;
        c.a = 1f;
        trickTextField.color = c;
    }

    private void AnimateButton(Transform buttonTransform)
    {
        if (buttonTransform == null) return;
        
        Tween.StopAll(buttonTransform);
        Tween.PunchScale(buttonTransform, strength: Vector3.one * buttonPunchScale, duration: 0.2f);
    }
}