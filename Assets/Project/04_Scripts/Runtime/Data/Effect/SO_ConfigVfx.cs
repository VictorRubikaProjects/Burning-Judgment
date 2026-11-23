using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game/Vfx", fileName = "SO_ConfigVfx")]
public class SO_ConfigVfx : ScriptableObject
{
    [field: SerializeField] public ParticleSystem VfxPrefabEjection { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabChainInputSuccess { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabAttackPlayer { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabHit { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabConfirmKill { get; private set; }
    [field: SerializeField] public ParticleSystem VfxPrefabDasherAttack { get; private set; }

    [SerializeField] private VfxDashData[] vfxPrefabsDash = Array.Empty<VfxDashData>();
    [SerializeField] private VfxStabilityData[] vfxPrefabsStability = Array.Empty<VfxStabilityData>();

    public ParticleSystem GetVfxDashByComboIndex(int combo)
    {
        return GetBestByThreshold(vfxPrefabsDash, combo, d => d.ComboThreshold, d => d.VfxDash, "dash");
    }

    public ParticleSystem GetVfxStability(float stability)
    {
        float clamped = Mathf.Clamp(stability, 0.1f, 1f);

        if (vfxPrefabsStability == null || vfxPrefabsStability.Length == 0)
            return null;

        foreach (var entry in vfxPrefabsStability)
        {
            if (entry == null || entry.VfxStability == null)
                continue;

            if (entry.Matches(clamped))
                return entry.VfxStability;
        }

        return null;
    }

    private ParticleSystem GetBestByThreshold<T>(
        T[] entries,
        float value,
        Func<T, float> getThreshold,
        Func<T, ParticleSystem> getPrefab,
        string label)
    {
        if (entries == null || entries.Length == 0)
        {
            Debug.LogWarning($"{nameof(SO_ConfigVfx)} ({name}): No {label} VFX entries configured.", this);
            return null;
        }

        ParticleSystem bestMatch = null;
        float highestMetThreshold = float.NegativeInfinity;

        foreach (var entry in entries)
        {
            if (entry == null) continue;

            float threshold = getThreshold(entry);
            if (value >= threshold && threshold >= highestMetThreshold)
            {
                highestMetThreshold = threshold;
                bestMatch = getPrefab(entry);
            }
        }

        return bestMatch != null ? bestMatch : getPrefab(entries[0]);
    }

    [Serializable]
    public class VfxDashData
    {
        [field: SerializeField] public ParticleSystem VfxDash { get; private set; }
        [field: SerializeField] public int ComboThreshold { get; private set; }
    }

    [Serializable]
    public class VfxStabilityData
    {
        [field: SerializeField] public ParticleSystem VfxStability { get; private set; }

        [field: SerializeField, Range(0.1f, 1f)] public float MinStability { get; private set; } = 0.1f;
        [field: SerializeField, Range(0.1f, 1f)] public float MaxStability { get; private set; } = 1f;

        public bool Matches(float stability)
        {
            return stability >= MinStability && stability <= MaxStability;
        }
    }
}