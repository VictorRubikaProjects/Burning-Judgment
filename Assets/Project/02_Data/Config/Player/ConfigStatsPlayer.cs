using UnityEngine;

[CreateAssetMenu(fileName = "ConfigStatsPlayer", menuName = "Config/Player Stats")]
public class ConfigStatsPlayer : ScriptableObject
{
    [Header("Dash")]
    public float dashDistance = 1f;
    public float dashDuration = 0.1f;

    [Header("Hit")]
    public float hitDuration = 0.3f;
    public int maxHealth = 3;

    [Header("Input")]
    public float swipeThreshold = 50f;

    [Header("Animation")]
    public float crossFadeDuration = 0.1f;
}