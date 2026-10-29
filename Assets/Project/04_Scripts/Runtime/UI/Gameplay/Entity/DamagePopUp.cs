using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class DamagePopUp : MonoBehaviour
{
    [SerializeField] private TMP_Text damageTextPrefab;
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float offsetY = 1f;
    
    private ObjectPool<TMP_Text> m_damagePool;
    private CameraService m_cameraService;

    private void Awake()
    {
        m_damagePool = new ObjectPool<TMP_Text>(
            createFunc: CreateItem,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyItem,
            collectionCheck: true,   
            defaultCapacity: poolSize,
            maxSize: 50
        );

        m_cameraService = ServiceLocator.Get<CameraService>();
    }

    public void Show(float damageAmount, Vector3 position)
    {
        TMP_Text popupText = m_damagePool.Get();
        popupText.transform.position = position + new Vector3(0, offsetY, 0);
        popupText.text = damageAmount.ToString();

        AnimateAndReleaseAsync(popupText, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid AnimateAndReleaseAsync(TMP_Text popupText, CancellationToken cancellationToken)
    {
        float elapsedTime = 0f;
        Color startColor = popupText.color;
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = Vector3.one * punchScale;

        try
        {
            while (elapsedTime < lifeTime)
            {
                if (cancellationToken.IsCancellationRequested || popupText == null) break;

                elapsedTime += Time.deltaTime;
                
                float normalizedTime = elapsedTime / lifeTime;

                popupText.transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

                popupText.transform.localScale = normalizedTime < 0.2f ?
                    Vector3.Lerp(originalScale, targetScale, normalizedTime / 0.2f) :
                    Vector3.Lerp(targetScale, originalScale, (normalizedTime - 0.2f) / 0.8f);

                Color color = startColor;
                
                color.a = Mathf.Lerp(1f, 0f, normalizedTime);
                
                popupText.color = color;

                popupText.transform.LookAt(
                    popupText.transform.position + m_cameraService.MainCamera.transform.rotation * Vector3.forward,
                    m_cameraService.MainCamera.transform.rotation * Vector3.up);
                
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            
        }

        if (popupText != null)
        {
            popupText.color = startColor;
            m_damagePool.Release(popupText);
        }
    }
    
    private TMP_Text CreateItem()
    {
        TMP_Text text = Instantiate(damageTextPrefab, transform);
        text.gameObject.SetActive(false);
        return text;
    }

    private void OnGet(TMP_Text pooledObject)
    {
        pooledObject.gameObject.SetActive(true);
    }

    private void OnRelease(TMP_Text pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
    }

    private void OnDestroyItem(TMP_Text pooledObject)
    {
        if (pooledObject != null)
        {
            Destroy(pooledObject.gameObject);
        }
    }
}