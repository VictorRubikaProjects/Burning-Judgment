using PrimeTween;
using UnityEngine;

[CreateAssetMenu(fileName = "HudConfig", menuName = "UI/Hud Config")]
public class SO_HudConfig : ScriptableObject
{
    [Header("Combo Animation")]
    public HudManager.ComboAnimationConfig comboAnimation = new()
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

    [Header("Combo Colors")]
    public HudManager.ComboColorConfig comboColors = new()
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