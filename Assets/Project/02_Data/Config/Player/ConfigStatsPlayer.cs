using UnityEngine;

[CreateAssetMenu(fileName = "ConfigStatsPlayer", menuName = "Config/Player Stats")]
public class ConfigStatsPlayer : ScriptableObject
{
    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public AnimationCurve dashCurve = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [Header("Hit")]
    public float hitDuration = 0.3f;
    public int maxHealth = 3;

    [Header("Input")]
    public float swipeThreshold = 50f;

    [Header("Animation")]
    public float crossFadeDuration = 0.1f;
}