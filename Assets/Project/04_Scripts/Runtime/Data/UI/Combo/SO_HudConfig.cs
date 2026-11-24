using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "HudConfig", menuName = "UI/Hud Config")]
public class SO_HudConfig : ScriptableObject
{
    [System.Serializable]
    public struct ComboAnimationConfig
    {
        [Header("Timing")]
        public float displayDuration;
        public float hideDuration;
        public float punchDuration;

        [Header("Effects")]
        public Vector3 punchScaleStrength;
        public float minTiltAngle;
        public float maxTiltAngle;
        public int tiltCycles;
        public CycleMode tiltCycleMode;
        public Ease animationEase;
    }

    [System.Serializable]
    public struct ComboColorConfig
    {
        public Color defaultColor;
        public Color tier1Color;
        public Color tier2Color;
        public Color tier3Color;
    }

    [Header("Combo Animation")]
    public ComboAnimationConfig comboAnimation = new()
    {
        displayDuration = 1.5f,
        hideDuration = 0.2f,
        punchDuration = 0.25f,
        punchScaleStrength = new Vector3(0.3f, 0.3f, 0.3f),
        minTiltAngle = -12f,
        maxTiltAngle = 12f,
        tiltCycles = 2,
        tiltCycleMode = CycleMode.Yoyo,
        animationEase = Ease.Default
    };

    [Header("Combo Dynamic Scaling")]
    [Tooltip("Multiplicateur d'échelle ajouté par point de combo.")]
    public float comboScaleGrowthRate = 0.04f;
    [Tooltip("Taille maximale du multiplicateur pour éviter un texte trop grand.")]
    public float maxComboScaleMultiplier = 2.5f;

    [Header("Combo Colors")]
    public ComboColorConfig comboColors = new()
    {
        defaultColor = Color.white,
        tier1Color = Color.yellow,
        tier2Color = new Color(1f, 0.5f, 0f),
        tier3Color = Color.red
    };

    [Header("Stability Animation")]
    public float stabilityPunchDuration = 0.25f;
    public Vector3 stabilityPunchScale = new Vector3(0.3f, 0.3f, 0.3f);
    public float stabilityMinTilt = 10f;
    public float stabilityMaxTilt = 20f;
}