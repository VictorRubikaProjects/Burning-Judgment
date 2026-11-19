using System;
using UnityEngine;

public class StabilityVisuals : MonoBehaviour
{
    [Header("Thresholds")]
    [SerializeField, Range(0.1f, 1f)] private float smokeThreshold = 0.7f;
    [SerializeField, Range(0.1f, 1f)] private float sparkThreshold = 0.4f;
    [SerializeField, Range(0.1f, 1f)] private float fireThreshold = 0.2f;
    
    private const int SmokeChildIndex = 0;
    private const int SparkChildIndex = 1;
    private const int FireChildIndex = 2;

    [SerializeField] private bool stopEmittingOnly = true;

    private ParticleSystem m_rootParticles;
    private StabilityVisualsData[] m_entries;

    private void Awake()
    {
        InitializeFromFxService();
        SetStabilityVisuals(1f);
    }

    private void OnDestroy()
    {
        if (m_rootParticles)
        {
            Destroy(m_rootParticles.gameObject);
            m_rootParticles = null;
        }
    }

    public void InitializeFromFxService()
    {
        FxService fxService = ServiceLocator.Get<FxService>();
        ParticleSystem rootPrefab = fxService?.ConfigVfx.VfxPrefabStability;

        if (rootPrefab != null)
        {
            m_rootParticles = Instantiate(rootPrefab, transform);
            m_rootParticles.transform.localPosition = Vector3.zero;
            m_rootParticles.transform.localRotation = Quaternion.identity;
        }

        m_entries = new[]
        {
            new StabilityVisualsData(GetChildParticleSystemAtIndex(SmokeChildIndex), smokeThreshold),
            new StabilityVisualsData(GetChildParticleSystemAtIndex(SparkChildIndex), sparkThreshold),
            new StabilityVisualsData(GetChildParticleSystemAtIndex(FireChildIndex), fireThreshold)
        };
    }

    private ParticleSystem GetChildParticleSystemAtIndex(int childIndex)
    {
        if (m_rootParticles == null)
            return null;

        Transform rootTransform = m_rootParticles.transform;
        if (childIndex < 0 || childIndex >= rootTransform.childCount)
        {
            Debug.LogWarning($"[StabilityVisuals] Index d'enfant '{childIndex}' hors limites sous '{m_rootParticles.name}' (enfants : {rootTransform.childCount}).", this);
            return null;
        }

        Transform child = rootTransform.GetChild(childIndex);
        ParticleSystem ps = child.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            Debug.LogWarning($"[StabilityVisuals] Aucun ParticleSystem trouvé à l'index '{childIndex}' sous '{m_rootParticles.name}'.", this);
        }

        return ps;
    }

    public void SetStabilityVisuals(float stability)
    {
        float clampedStability = Mathf.Clamp(stability, 0.1f, 1f);

        if (m_entries == null || m_entries.Length == 0)
            return;

        bool anyActive = false;

        foreach (StabilityVisualsData entry in m_entries)
        {
            if (!entry.Particle) continue;

            bool shouldBeActive = clampedStability <= entry.Threshold;
            SetParticleState(entry.Particle, shouldBeActive);

            if (shouldBeActive)
                anyActive = true;
        }

        if (m_rootParticles)
        {
            if (anyActive && !m_rootParticles.isPlaying)
                m_rootParticles.Play(false);
            else if (!anyActive && m_rootParticles.isPlaying)
                m_rootParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void SetParticleState(ParticleSystem ps, bool active)
    {
        if (active)
        {
            if (!ps.gameObject.activeSelf)
                ps.gameObject.SetActive(true);

            if (!ps.isPlaying)
                ps.Play(false);
        }
        else if (ps.gameObject.activeSelf)
        {
            if (stopEmittingOnly)
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            else
            {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }
        }
    }

    private void OnValidate()
    {
        smokeThreshold = Mathf.Clamp(smokeThreshold, 0.1f, 1f);
        sparkThreshold = Mathf.Clamp(sparkThreshold, 0.1f, smokeThreshold);
        fireThreshold = Mathf.Clamp(fireThreshold, 0.1f, sparkThreshold);
    }

    [Serializable]
    private sealed class StabilityVisualsData
    {
        public ParticleSystem Particle { get; }
        public float Threshold { get; }

        public StabilityVisualsData(ParticleSystem particle, float threshold)
        {
            Particle = particle;
            Threshold = threshold;
        }
    }
}