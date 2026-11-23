using UnityEngine;

public class StabilityVisuals : MonoBehaviour
{
    [SerializeField] private bool stopEmittingOnly = true;

    private SO_ConfigVfx m_configVfx;
    private ParticleSystem m_currentPrefab;
    private ParticleSystem m_currentInstance;

    private void Awake()
    {
        m_configVfx = ServiceLocator.Get<FxService>()?.ConfigVfx;
        SetStabilityVisuals(1f);
    }

    private void OnDestroy()
    {
        if (m_currentInstance != null)
        {
            Destroy(m_currentInstance.gameObject);
            m_currentInstance = null;
        }
    }

    public void SetStabilityVisuals(float stability)
    {
        ParticleSystem targetPrefab = m_configVfx != null
            ? m_configVfx.GetVfxStability(stability)
            : null;

        if (targetPrefab == m_currentPrefab)
            return;

        if (m_currentInstance != null)
        {
            if (stopEmittingOnly)
            {
                m_currentInstance.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(m_currentInstance.gameObject, 2f);
            }
            else
            {
                Destroy(m_currentInstance.gameObject);
            }

            m_currentInstance = null;
        }

        m_currentPrefab = targetPrefab;

        if (targetPrefab != null)
        {
            m_currentInstance = Instantiate(targetPrefab, transform);
            m_currentInstance.transform.localPosition = Vector3.zero;
            m_currentInstance.transform.localRotation = Quaternion.identity;
            m_currentInstance.Play(true);
        }
    }
}